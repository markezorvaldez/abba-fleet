using AbbaFleet.Features.Trucks;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Trucks;

public class InvestmentEntryTests
{
    private readonly IFixture _fixture = CreateFixture();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));

        return fixture;
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyCreatedBy_Throws(string createdBy)
    {
        var truckId = _fixture.Create<Guid>();

        Assert.Throws<ArgumentException>(() =>
            new InvestmentEntry(truckId, InvestmentType.Repair, 100m, _fixture.Create<DateOnly>(), null, createdBy));
    }

    [Fact]
    public void Constructor_EmptyTruckId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new InvestmentEntry(Guid.Empty, InvestmentType.Repair, 100m, _fixture.Create<DateOnly>(), null, "user"));
    }

    [Fact]
    public void Constructor_NegativeAmount_Throws()
    {
        var truckId = _fixture.Create<Guid>();

        Assert.Throws<ArgumentException>(() =>
            new InvestmentEntry(truckId, InvestmentType.Repair, -50m, _fixture.Create<DateOnly>(), null, "user"));
    }

    [Fact]
    public void Constructor_NullDescription_StaysNull()
    {
        var truckId = _fixture.Create<Guid>();

        var entry = new InvestmentEntry(truckId, InvestmentType.Purchase, 1000m, _fixture.Create<DateOnly>(), null, "user");

        Assert.Null(entry.Description);
    }

    [Fact]
    public void Constructor_TrimsDescription()
    {
        var truckId = _fixture.Create<Guid>();
        var desc = "  some description  ";

        var entry = new InvestmentEntry(truckId, InvestmentType.Other, 100m, _fixture.Create<DateOnly>(), desc, "user");

        Assert.Equal("some description", entry.Description);
    }

    [Fact]
    public void Constructor_ValidInput_SetsAllProperties()
    {
        var truckId = _fixture.Create<Guid>();
        var type = InvestmentType.Repair;
        var amount = 500.00m;
        var date = _fixture.Create<DateOnly>();
        var description = "  Fixed brakes  ";
        var createdBy = "  testuser  ";

        var entry = new InvestmentEntry(truckId, type, amount, date, description, createdBy);

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(truckId, entry.TruckId);
        Assert.Equal(type, entry.Type);
        Assert.Equal(amount, entry.Amount);
        Assert.Equal(date, entry.Date);
        Assert.Equal("Fixed brakes", entry.Description);
        Assert.Equal("testuser", entry.CreatedBy);
        Assert.True(entry.CreatedAt > DateTimeOffset.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void Constructor_ZeroAmount_Throws()
    {
        var truckId = _fixture.Create<Guid>();

        Assert.Throws<ArgumentException>(() =>
            new InvestmentEntry(truckId, InvestmentType.Repair, 0m, _fixture.Create<DateOnly>(), null, "user"));
    }
}
