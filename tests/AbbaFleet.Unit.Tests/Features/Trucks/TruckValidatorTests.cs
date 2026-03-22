using AbbaFleet.Features.Trucks;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Trucks;

public class TruckValidatorTests
{
    private readonly IFixture _fixture = CreateFixture();
    private readonly TruckValidator _validator = new();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));
        return fixture;
    }

    [Fact]
    public void ValidRequest_Passes()
    {
        var request = new UpsertTruckRequest(
            _fixture.Create<string>()[..10],
            _fixture.Create<string>(),
            OwnershipType.CompanyOwned,
            null,
            _fixture.Create<DateOnly>());

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyPlateNumber_Fails(string? plateNumber)
    {
        var request = new UpsertTruckRequest(
            plateNumber!,
            _fixture.Create<string>(),
            OwnershipType.CompanyOwned,
            null,
            _fixture.Create<DateOnly>());

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PlateNumber");
    }

    [Fact]
    public void PlateNumberTooLong_Fails()
    {
        var request = new UpsertTruckRequest(
            new string('A', 21),
            _fixture.Create<string>(),
            OwnershipType.CompanyOwned,
            null,
            _fixture.Create<DateOnly>());

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PlateNumber");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyTruckModel_Fails(string? truckModel)
    {
        var request = new UpsertTruckRequest(
            _fixture.Create<string>()[..10],
            truckModel!,
            OwnershipType.CompanyOwned,
            null,
            _fixture.Create<DateOnly>());

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TruckModel");
    }

    [Fact]
    public void TruckModelTooLong_Fails()
    {
        var request = new UpsertTruckRequest(
            _fixture.Create<string>()[..10],
            new string('A', 101),
            OwnershipType.CompanyOwned,
            null,
            _fixture.Create<DateOnly>());

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TruckModel");
    }
}
