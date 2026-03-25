using AbbaFleet.Features.Drivers;
using AbbaFleet.Shared;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Drivers;

public class DriverServiceTests
{
    private readonly IFixture _fixture = CreateFixture();

    private readonly IDriverRepository _repository = Substitute.For<IDriverRepository>();
    private readonly IValidator<UpsertDriverRequest> _validator = Substitute.For<IValidator<UpsertDriverRequest>>();
    private readonly IFileRepository _fileRepository = Substitute.For<IFileRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));

        return fixture;
    }

    [Fact]
    public async Task CreateAsync_InvalidInput_ReturnsFailureAndDoesNotAddToRepository()
    {
        var request = _fixture.Create<UpsertDriverRequest>();
        var errorMessage = _fixture.Create<string>();

        _validator.ValidateAsync(
                      Arg.Is<UpsertDriverRequest>(r =>
                          r.FullName == request.FullName && r.PhoneNumber == request.PhoneNumber))
                  .Returns(
                      new ValidationResult(
                      [
                          new ValidationFailure("FullName", errorMessage)
                      ]));

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);

        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Contains(errorMessage, result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Driver>());
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccessAndAddsToRepository()
    {
        var request = _fixture.Create<UpsertDriverRequest>();

        _validator.ValidateAsync(
                      Arg.Is<UpsertDriverRequest>(r =>
                          r.FullName == request.FullName && r.PhoneNumber == request.PhoneNumber))
                  .Returns(new ValidationResult());

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);

        var result = await service.CreateAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal(request.FullName, result.Value!.FullName);

        await _repository.Received(1)
                         .AddAsync(
                             Arg.Is<Driver>(d =>
                                 d.FullName == request.FullName && d.PhoneNumber == request.PhoneNumber));
    }

    // --- DeactivateAsync ---

    [Fact]
    public async Task DeactivateAsync_ActiveDriver_DeactivatesAndCallsRepository()
    {
        var request = _fixture.Create<UpsertDriverRequest>();

        var driver = new Driver(
            request.FullName,
            request.PhoneNumber,
            null,
            null,
            false,
            request.DateStarted);

        var reason = _fixture.Create<string>();
        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.DeactivateAsync(driver.Id, reason);

        Assert.True(result.Succeeded);

        await _repository.Received(1)
                         .UpdateAsync(
                             Arg.Is<Driver>(d =>
                                 d.Id == driver.Id && !d.IsActive));
    }

    [Fact]
    public async Task DeactivateAsync_AlreadyInactive_ReturnsFailure()
    {
        var request = _fixture.Create<UpsertDriverRequest>();

        var driver = new Driver(
            request.FullName,
            request.PhoneNumber,
            null,
            null,
            false,
            request.DateStarted);

        driver.Update(driver.FullName, driver.PhoneNumber, null, null, isActive: false, driver.IsReliever, driver.DateStarted);
        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.DeactivateAsync(driver.Id, _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("already inactive", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Driver>());
    }

    [Fact]
    public async Task DeactivateAsync_DriverNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Driver?)null);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.DeactivateAsync(id, _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Driver>());
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_DriverExists_ReturnsSuccessAndCallsRepository()
    {
        var request = _fixture.Create<UpsertDriverRequest>();

        var driver = new Driver(
            request.FullName,
            request.PhoneNumber,
            null,
            null,
            false,
            request.DateStarted);

        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.DeleteAsync(driver.Id);

        Assert.True(result.Succeeded);
        await _repository.Received(1).DeleteAsync(Arg.Is<Driver>(d => d.Id == driver.Id));
    }

    [Fact]
    public async Task DeleteAsync_DriverNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns((Driver?)null);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.DeleteAsync(id);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Driver>());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedSummaries()
    {
        var name1 = _fixture.Create<string>();
        var phone1 = _fixture.Create<string>();
        var name2 = _fixture.Create<string>();
        var phone2 = _fixture.Create<string>();
        var date1 = _fixture.Create<DateOnly>();
        var date2 = _fixture.Create<DateOnly>();

        var drivers = new List<Driver>
        {
            new(name1, phone1, null, null, false, date1),
            new(name2, phone2, null, null, true, date2)
        };

        _repository.GetAllAsync().Returns(drivers);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var summaries = await service.GetAllAsync();

        Assert.Equal(2, summaries.Count);
        Assert.Equal(name1, summaries[0].FullName);
        Assert.Equal(name2, summaries[1].FullName);
        Assert.True(summaries[1].IsReliever);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_DriverExists_ReturnsMappedDetail()
    {
        var request = _fixture.Create<UpsertDriverRequest>();

        var driver = new Driver(
            request.FullName,
            request.PhoneNumber,
            request.FacebookLink,
            request.Address,
            request.IsReliever,
            request.DateStarted);

        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var detail = await service.GetByIdAsync(driver.Id);

        Assert.NotNull(detail);
        Assert.Equal(request.FullName, detail.FullName);
        Assert.Equal(request.PhoneNumber, detail.PhoneNumber);
        Assert.Equal(request.FacebookLink, detail.FacebookLink);
        Assert.Equal(request.Address, detail.Address);
        Assert.Equal(request.IsReliever, detail.IsReliever);
    }

    [Fact]
    public async Task GetByIdAsync_DriverNotFound_ReturnsNull()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns((Driver?)null);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var detail = await service.GetByIdAsync(id);

        Assert.Null(detail);
    }

    [Fact]
    public async Task ReactivateAsync_AlreadyActive_ReturnsFailure()
    {
        var request = _fixture.Create<UpsertDriverRequest>();

        var driver = new Driver(
            request.FullName,
            request.PhoneNumber,
            null,
            null,
            false,
            request.DateStarted);

        // Driver is active by default after construction
        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.ReactivateAsync(driver.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("already active", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Driver>());
    }

    [Fact]
    public async Task ReactivateAsync_DriverNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Driver?)null);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.ReactivateAsync(id);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Driver>());
    }

    // --- ReactivateAsync ---

    [Fact]
    public async Task ReactivateAsync_InactiveDriver_ReactivatesAndCallsRepository()
    {
        var request = _fixture.Create<UpsertDriverRequest>();

        var driver = new Driver(
            request.FullName,
            request.PhoneNumber,
            null,
            null,
            false,
            request.DateStarted);

        driver.Update(driver.FullName, driver.PhoneNumber, null, null, isActive: false, driver.IsReliever, driver.DateStarted);
        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.ReactivateAsync(driver.Id);

        Assert.True(result.Succeeded);

        await _repository.Received(1)
                         .UpdateAsync(
                             Arg.Is<Driver>(d =>
                                 d.Id == driver.Id && d.IsActive));
    }

    [Fact]
    public async Task UpdateAsync_DriverNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        var request = _fixture.Create<UpsertDriverRequest>();

        _validator.ValidateAsync(
                      Arg.Is<UpsertDriverRequest>(r =>
                          r.FullName == request.FullName && r.PhoneNumber == request.PhoneNumber))
                  .Returns(new ValidationResult());

        _repository.GetByIdAsync(Arg.Is(id)).Returns((Driver?)null);

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.UpdateAsync(id, request);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Driver>());
    }

    [Fact]
    public async Task UpdateAsync_InvalidInput_ReturnsFailure_DoesNotCallRepository()
    {
        var request = _fixture.Create<UpsertDriverRequest>();
        var errorMessage = _fixture.Create<string>();

        _validator.ValidateAsync(
                      Arg.Is<UpsertDriverRequest>(r =>
                          r.FullName == request.FullName && r.PhoneNumber == request.PhoneNumber))
                  .Returns(new ValidationResult([new ValidationFailure("FullName", errorMessage)]));

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.UpdateAsync(_fixture.Create<Guid>(), request);

        Assert.False(result.Succeeded);
        Assert.Contains(errorMessage, result.Error);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Driver>());
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidInput_ReturnsSuccessAndCallsRepository()
    {
        var createRequest = _fixture.Create<UpsertDriverRequest>();

        var driver = new Driver(
            createRequest.FullName,
            createRequest.PhoneNumber,
            null,
            null,
            false,
            createRequest.DateStarted);

        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var updateRequest = _fixture.Create<UpsertDriverRequest>();

        _validator.ValidateAsync(
                      Arg.Is<UpsertDriverRequest>(r =>
                          r.FullName == updateRequest.FullName && r.PhoneNumber == updateRequest.PhoneNumber))
                  .Returns(new ValidationResult());

        var service = new DriverService(_validator, _repository, _fileRepository, _fileStorageService);
        var result = await service.UpdateAsync(driver.Id, updateRequest);

        Assert.True(result.Succeeded);
        Assert.Equal(updateRequest.FullName, result.Value!.FullName);
        Assert.Equal(updateRequest.PhoneNumber, result.Value.PhoneNumber);
        Assert.Equal(updateRequest.IsReliever, result.Value.IsReliever);

        await _repository.Received(1)
                         .UpdateAsync(
                             Arg.Is<Driver>(d =>
                                 d.FullName == updateRequest.FullName && d.PhoneNumber == updateRequest.PhoneNumber));
    }
}
