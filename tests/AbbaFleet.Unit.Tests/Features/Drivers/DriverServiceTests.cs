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
    private readonly IValidator<UpsertDriverRequest> _validator = Substitute.For<IValidator<UpsertDriverRequest>>();

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccessAndAddsToRepository()
    {
        var request = _fixture.Create<UpsertDriverRequest>();

        _validator.Validate(Arg.Is<UpsertDriverRequest>(r =>
            r.FullName == request.FullName && r.PhoneNumber == request.PhoneNumber))
            .Returns(new ValidationResult());
        var service = new DriverService(_validator, _repository);

        var result = await service.CreateAsync(request);

        Assert.True(result.Succeeded);
        Assert.Equal(request.FullName, result.Value!.FullName);
        await _repository.Received(1).AddAsync(Arg.Is<Driver>(d =>
            d.FullName == request.FullName && d.PhoneNumber == request.PhoneNumber));
    }

    [Fact]
    public async Task CreateAsync_InvalidInput_ReturnsFailureAndDoesNotAddToRepository()
    {
        var request = _fixture.Create<UpsertDriverRequest>();
        var errorMessage = _fixture.Create<string>();

        _validator.Validate(Arg.Is<UpsertDriverRequest>(r =>
            r.FullName == request.FullName && r.PhoneNumber == request.PhoneNumber))
            .Returns(new ValidationResult(
        [
            new ValidationFailure("FullName", errorMessage)
        ]));
        var service = new DriverService(_validator, _repository);

        var result = await service.CreateAsync(request);

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

        var drivers = new List<Driver>
        {
            Driver.Create(name1, phone1, null, null, false, date1),
            Driver.Create(name2, phone2, null, null, true, date2)
        };
        _repository.GetAllAsync().Returns(drivers);

        var service = new DriverService(_validator, _repository);
        var summaries = await service.GetAllAsync();

        Assert.Equal(2, summaries.Count);
        Assert.Equal(name1, summaries[0].FullName);
        Assert.Equal(name2, summaries[1].FullName);
        Assert.True(summaries[1].IsReliever);
    }
}
