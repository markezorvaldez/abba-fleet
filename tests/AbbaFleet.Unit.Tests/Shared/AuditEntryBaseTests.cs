using AbbaFleet.Features.Drivers;
using AbbaFleet.Shared;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Shared;

public class AuditEntryBaseTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        var driverId = _fixture.Create<Guid>();
        var actionType = AuditActionType.Updated;
        var changedBy = _fixture.Create<string>();
        var reason = _fixture.Create<string>();

        var entry = new DriverAuditEntry(driverId, actionType, changedBy, reason);

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(driverId, entry.DriverId);
        Assert.Equal(actionType, entry.ActionType);
        Assert.Equal(changedBy, entry.ChangedBy);
        Assert.Equal(reason, entry.Reason);
        Assert.True(entry.Timestamp > DateTimeOffset.MinValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyChangedBy_Throws(string? changedBy)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new DriverAuditEntry(_fixture.Create<Guid>(), AuditActionType.Created, changedBy!));
    }

    [Fact]
    public void Constructor_EmptyDriverId_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new DriverAuditEntry(Guid.Empty, AuditActionType.Created, _fixture.Create<string>()));
    }

    [Fact]
    public void Constructor_NullOptionalFields_Allowed()
    {
        var entry = new DriverAuditEntry(
            _fixture.Create<Guid>(), AuditActionType.Created, _fixture.Create<string>());

        Assert.Null(entry.Reason);
    }

    [Fact]
    public void Constructor_SetsIdAndTimestamp()
    {
        var before = DateTimeOffset.UtcNow;

        var entry = new DriverAuditEntry(
            _fixture.Create<Guid>(), AuditActionType.Created, _fixture.Create<string>());

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.True(entry.Timestamp >= before);
    }
}
