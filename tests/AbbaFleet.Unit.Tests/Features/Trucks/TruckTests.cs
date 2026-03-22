using AbbaFleet.Features.Trucks;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Trucks;

public class TruckTests
{
    private readonly IFixture _fixture = CreateFixture();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));
        return fixture;
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var plate = _fixture.Create<string>();
        var model = _fixture.Create<string>();

        var truck = new Truck(
            $"  {plate}  ", $"  {model}  ",
            OwnershipType.CompanyOwned, null, _fixture.Create<DateOnly>());

        Assert.Equal(plate, truck.PlateNumber);
        Assert.Equal(model, truck.TruckModel);
    }

    [Fact]
    public void Create_SetsExpectedDefaults()
    {
        var plate = _fixture.Create<string>();
        var model = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();
        var driverId = _fixture.Create<Guid>();

        var truck = new Truck(plate, model, OwnershipType.Subcontracted, driverId, date);

        Assert.True(truck.IsActive);
        Assert.Equal(OwnershipType.Subcontracted, truck.OwnershipType);
        Assert.Equal(driverId, truck.DriverId);
        Assert.Equal(date, truck.DateAcquired);
        Assert.NotEqual(Guid.Empty, truck.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnEmptyPlateNumber(string plateNumber)
    {
        Assert.Throws<ArgumentException>(() =>
            new Truck(plateNumber, _fixture.Create<string>(),
                OwnershipType.CompanyOwned, null, _fixture.Create<DateOnly>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnEmptyTruckModel(string truckModel)
    {
        Assert.Throws<ArgumentException>(() =>
            new Truck(_fixture.Create<string>(), truckModel,
                OwnershipType.CompanyOwned, null, _fixture.Create<DateOnly>()));
    }

    // --- Update ---

    private Truck CreateTruck() =>
        new Truck(
            _fixture.Create<string>(), _fixture.Create<string>(),
            OwnershipType.CompanyOwned, null, _fixture.Create<DateOnly>());

    [Fact]
    public void Update_SetsAllFields()
    {
        var truck = CreateTruck();
        var newPlate = _fixture.Create<string>();
        var newModel = _fixture.Create<string>();
        var newDate = _fixture.Create<DateOnly>();
        var newDriverId = _fixture.Create<Guid>();

        truck.Update(newPlate, newModel, OwnershipType.Subcontracted,
            newDriverId, isActive: false, newDate);

        Assert.Equal(newPlate, truck.PlateNumber);
        Assert.Equal(newModel, truck.TruckModel);
        Assert.Equal(OwnershipType.Subcontracted, truck.OwnershipType);
        Assert.Equal(newDriverId, truck.DriverId);
        Assert.False(truck.IsActive);
        Assert.Equal(newDate, truck.DateAcquired);
    }

    [Fact]
    public void Update_TrimsWhitespace()
    {
        var truck = CreateTruck();
        var newPlate = _fixture.Create<string>();
        var newModel = _fixture.Create<string>();

        truck.Update($"  {newPlate}  ", $"  {newModel}  ",
            OwnershipType.CompanyOwned, null, true, _fixture.Create<DateOnly>());

        Assert.Equal(newPlate, truck.PlateNumber);
        Assert.Equal(newModel, truck.TruckModel);
    }

    [Fact]
    public void Update_SetsUpdatedAt()
    {
        var truck = CreateTruck();
        var beforeUpdate = truck.UpdatedAt;

        truck.Update(_fixture.Create<string>(), _fixture.Create<string>(),
            OwnershipType.CompanyOwned, null, true, _fixture.Create<DateOnly>());

        Assert.True(truck.UpdatedAt >= beforeUpdate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ThrowsOnEmptyPlateNumber(string plateNumber)
    {
        var truck = CreateTruck();

        Assert.Throws<ArgumentException>(() =>
            truck.Update(plateNumber, _fixture.Create<string>(),
                OwnershipType.CompanyOwned, null, true, _fixture.Create<DateOnly>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ThrowsOnEmptyTruckModel(string truckModel)
    {
        var truck = CreateTruck();

        Assert.Throws<ArgumentException>(() =>
            truck.Update(_fixture.Create<string>(), truckModel,
                OwnershipType.CompanyOwned, null, true, _fixture.Create<DateOnly>()));
    }
}
