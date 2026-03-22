using AbbaFleet.Features.Drivers;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Drivers;

public class DriverServiceTests
{
    private readonly IFixture _fixture = CreateFixture();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));
        return fixture;
    }
    private readonly IDriverRepository _repository = Substitute.For<IDriverRepository>();
    private readonly IValidator<Driver> _validator = Substitute.For<IValidator<Driver>>();

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccessAndAddsToRepository()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();

        _validator.Validate(Arg.Is<Driver>(d =>
            d.FullName == name && d.PhoneNumber == phone))
            .Returns(new ValidationResult());
        var service = new DriverService(_validator, _repository);

        var result = await service.CreateAsync(name, phone, null, null, false, date);

        Assert.True(result.Succeeded);
        Assert.Equal(name, result.Value!.FullName);
        await _repository.Received(1).AddAsync(Arg.Is<Driver>(d =>
            d.FullName == name && d.PhoneNumber == phone));
    }

    [Fact]
    public async Task CreateAsync_InvalidInput_ReturnsFailureAndDoesNotAddToRepository()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var errorMessage = _fixture.Create<string>();
        _validator.Validate(Arg.Is<Driver>(d =>
            d.FullName == name && d.PhoneNumber == phone)).Returns(new ValidationResult(
        [
            new ValidationFailure("FullName", errorMessage)
        ]));
        var service = new DriverService(_validator, _repository);

        var result = await service.CreateAsync(
            name,
            phone,
            null,
            null,
            false,
            _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
        Assert.Contains(errorMessage, result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Driver>());
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

        _validator.Validate(Arg.Any<Driver>()).Returns(new ValidationResult());
        var drivers = new List<Driver>
        {
            Driver.TryCreate(_validator, name1, phone1, null, null, false, date1).Value!,
            Driver.TryCreate(_validator, name2, phone2, null, null, true, date2).Value!
        };
        _repository.GetAllAsync().Returns(drivers);

        var service = new DriverService(_validator, _repository);
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
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var facebook = _fixture.Create<string>();
        var address = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();

        _validator.Validate(Arg.Any<Driver>()).Returns(new ValidationResult());
        var driver = Driver.TryCreate(_validator, name, phone, facebook, address, true, date).Value!;
        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository);
        var detail = await service.GetByIdAsync(driver.Id);

        Assert.NotNull(detail);
        Assert.Equal(name, detail.FullName);
        Assert.Equal(phone, detail.PhoneNumber);
        Assert.Equal(facebook, detail.FacebookLink);
        Assert.Equal(address, detail.Address);
        Assert.True(detail.IsReliever);
    }

    [Fact]
    public async Task GetByIdAsync_DriverNotFound_ReturnsNull()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns((Driver?)null);

        var service = new DriverService(_validator, _repository);
        var detail = await service.GetByIdAsync(id);

        Assert.Null(detail);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidInput_ReturnsSuccessAndCallsRepository()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();
        var newName = _fixture.Create<string>();
        var newPhone = _fixture.Create<string>();
        var newDate = _fixture.Create<DateOnly>();

        _validator.Validate(Arg.Any<Driver>()).Returns(new ValidationResult());
        var driver = Driver.TryCreate(_validator, name, phone, null, null, false, date).Value!;
        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository);
        var result = await service.UpdateAsync(
            driver.Id, newName, newPhone, null, null, false, true, newDate);

        Assert.True(result.Succeeded);
        Assert.Equal(newName, result.Value!.FullName);
        Assert.Equal(newPhone, result.Value.PhoneNumber);
        Assert.True(result.Value.IsReliever);
        await _repository.Received(1).UpdateAsync(Arg.Is<Driver>(d =>
            d.FullName == newName && d.PhoneNumber == newPhone));
    }

    [Fact]
    public async Task UpdateAsync_DriverNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns((Driver?)null);

        var service = new DriverService(_validator, _repository);
        var result = await service.UpdateAsync(
            id, _fixture.Create<string>(), _fixture.Create<string>(),
            null, null, true, false, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Driver>());
    }

    [Fact]
    public async Task UpdateAsync_InvalidInput_ReturnsFailure_DoesNotCallRepository()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();
        var errorMessage = _fixture.Create<string>();

        // First validate call (TryCreate) succeeds
        _validator.Validate(Arg.Any<Driver>()).Returns(
            new ValidationResult(),
            new ValidationResult([new ValidationFailure("FullName", errorMessage)]));

        var driver = Driver.TryCreate(_validator, name, phone, null, null, false, date).Value!;
        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository);
        var result = await service.UpdateAsync(
            driver.Id, "", phone, null, null, true, false, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
        Assert.Contains(errorMessage, result.Error);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Driver>());
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_DriverExists_ReturnsSuccessAndCallsRepository()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();

        _validator.Validate(Arg.Any<Driver>()).Returns(new ValidationResult());
        var driver = Driver.TryCreate(_validator, name, phone, null, null, false, date).Value!;
        _repository.GetByIdAsync(Arg.Is(driver.Id)).Returns(driver);

        var service = new DriverService(_validator, _repository);
        var result = await service.DeleteAsync(driver.Id);

        Assert.True(result.Succeeded);
        await _repository.Received(1).DeleteAsync(Arg.Is<Driver>(d => d.Id == driver.Id));
    }

    [Fact]
    public async Task DeleteAsync_DriverNotFound_ReturnsFailure()
    {
        var id = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(id)).Returns((Driver?)null);

        var service = new DriverService(_validator, _repository);
        var result = await service.DeleteAsync(id);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Driver>());
    }
}
