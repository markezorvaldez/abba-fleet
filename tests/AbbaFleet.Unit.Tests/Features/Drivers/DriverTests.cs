using AbbaFleet.Features.Drivers;
using AbbaFleet.Shared;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Drivers;

public class DriverTests
{
    private readonly IFixture _fixture = CreateFixture();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));

        return fixture;
    }

    // --- Update ---

    private Driver CreateDriver()
    {
        return new Driver(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            false,
            _fixture.Create<DateOnly>(),
            _fixture.Create<string>());
    }

    [Fact]
    public void ClearAuditLog_EmptiesCollection()
    {
        var driver = CreateDriver();
        Assert.NotEmpty(driver.AuditLog);

        driver.ClearAuditLog();

        Assert.Empty(driver.AuditLog);
    }

    // --- Audit ---

    [Fact]
    public void Constructor_AppendsCreatedAuditEntry()
    {
        var changedBy = _fixture.Create<string>();

        var driver = new Driver(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            null,
            null,
            false,
            _fixture.Create<DateOnly>(),
            changedBy);

        Assert.Single(driver.AuditLog);
        var entry = Assert.IsType<DriverAuditEntry>(driver.AuditLog.First());
        Assert.Equal(AuditActionType.Created, entry.ActionType);
        Assert.Equal(changedBy, entry.ChangedBy);
        Assert.Equal(driver.Id, entry.DriverId);
    }

    [Fact]
    public void Create_SetsExpectedDefaults()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();

        var driver = new Driver(name, phone, null, null, true, date, _fixture.Create<string>());

        Assert.True(driver.IsActive);
        Assert.True(driver.IsReliever);
        Assert.Equal(date, driver.DateStarted);
        Assert.NotEqual(Guid.Empty, driver.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnEmptyFullName(string fullName)
    {
        Assert.Throws<ArgumentException>(() =>
            new Driver(fullName, _fixture.Create<string>(), null, null, false, _fixture.Create<DateOnly>(), _fixture.Create<string>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnEmptyPhoneNumber(string phoneNumber)
    {
        Assert.Throws<ArgumentException>(() =>
            new Driver(_fixture.Create<string>(), phoneNumber, null, null, false, _fixture.Create<DateOnly>(), _fixture.Create<string>()));
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var facebook = _fixture.Create<string>();
        var address = _fixture.Create<string>();

        var driver = new Driver(
            $"  {name}  ",
            $"  {phone}  ",
            $"  {facebook}  ",
            $"  {address}  ",
            false,
            _fixture.Create<DateOnly>(),
            _fixture.Create<string>());

        Assert.Equal(name, driver.FullName);
        Assert.Equal(phone, driver.PhoneNumber);
        Assert.Equal(facebook, driver.FacebookLink);
        Assert.Equal(address, driver.Address);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse_AppendsDeactivatedEntry()
    {
        var driver = CreateDriver();
        driver.ClearAuditLog();

        var changedBy = _fixture.Create<string>();
        var reason = _fixture.Create<string>();

        driver.Deactivate(changedBy, reason);

        Assert.False(driver.IsActive);
        Assert.Single(driver.AuditLog);
        var entry = Assert.IsType<DriverAuditEntry>(driver.AuditLog.First());
        Assert.Equal(AuditActionType.Deactivated, entry.ActionType);
        Assert.Equal(changedBy, entry.ChangedBy);
        Assert.Equal(reason, entry.Reason);
    }

    [Fact]
    public void Reactivate_SetsIsActiveTrue_AppendsReactivatedEntry()
    {
        var driver = CreateDriver();
        driver.Deactivate(_fixture.Create<string>());
        driver.ClearAuditLog();

        var changedBy = _fixture.Create<string>();

        driver.Reactivate(changedBy);

        Assert.True(driver.IsActive);
        Assert.Single(driver.AuditLog);
        var entry = Assert.IsType<DriverAuditEntry>(driver.AuditLog.First());
        Assert.Equal(AuditActionType.Reactivated, entry.ActionType);
        Assert.Equal(changedBy, entry.ChangedBy);
    }

    [Fact]
    public void Update_AppendsUpdatedAuditEntry()
    {
        var driver = CreateDriver();
        driver.ClearAuditLog();

        var changedBy = _fixture.Create<string>();

        driver.Update(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            null,
            null,
            true,
            false,
            _fixture.Create<DateOnly>(),
            changedBy);

        Assert.Single(driver.AuditLog);
        var entry = Assert.IsType<DriverAuditEntry>(driver.AuditLog.First());
        Assert.Equal(AuditActionType.Updated, entry.ActionType);
        Assert.Equal(changedBy, entry.ChangedBy);
    }

    [Fact]
    public void Update_SetsAllFields()
    {
        var driver = CreateDriver();
        var newName = _fixture.Create<string>();
        var newPhone = _fixture.Create<string>();
        var newFacebook = _fixture.Create<string>();
        var newAddress = _fixture.Create<string>();
        var newDate = _fixture.Create<DateOnly>();

        driver.Update(
            newName,
            newPhone,
            newFacebook,
            newAddress,
            isActive: false,
            isReliever: true,
            newDate,
            _fixture.Create<string>());

        Assert.Equal(newName, driver.FullName);
        Assert.Equal(newPhone, driver.PhoneNumber);
        Assert.Equal(newFacebook, driver.FacebookLink);
        Assert.Equal(newAddress, driver.Address);
        Assert.False(driver.IsActive);
        Assert.True(driver.IsReliever);
        Assert.Equal(newDate, driver.DateStarted);
    }

    [Fact]
    public void Update_SetsUpdatedAt()
    {
        var driver = CreateDriver();
        var beforeUpdate = driver.UpdatedAt;

        driver.Update(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            null,
            null,
            true,
            false,
            _fixture.Create<DateOnly>(),
            _fixture.Create<string>());

        Assert.True(driver.UpdatedAt >= beforeUpdate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ThrowsOnEmptyFullName(string fullName)
    {
        var driver = CreateDriver();

        Assert.Throws<ArgumentException>(() =>
            driver.Update(fullName, _fixture.Create<string>(), null, null, true, false, _fixture.Create<DateOnly>(), _fixture.Create<string>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ThrowsOnEmptyPhoneNumber(string phoneNumber)
    {
        var driver = CreateDriver();

        Assert.Throws<ArgumentException>(() =>
            driver.Update(_fixture.Create<string>(), phoneNumber, null, null, true, false, _fixture.Create<DateOnly>(), _fixture.Create<string>()));
    }

    [Fact]
    public void Update_TrimsWhitespace()
    {
        var driver = CreateDriver();
        var newName = _fixture.Create<string>();
        var newPhone = _fixture.Create<string>();

        driver.Update(
            $"  {newName}  ",
            $"  {newPhone}  ",
            null,
            null,
            true,
            false,
            _fixture.Create<DateOnly>(),
            _fixture.Create<string>());

        Assert.Equal(newName, driver.FullName);
        Assert.Equal(newPhone, driver.PhoneNumber);
    }

    [Fact]
    public void Update_WithReason_IncludesReasonInAuditEntry()
    {
        var driver = CreateDriver();
        driver.ClearAuditLog();

        var reason = _fixture.Create<string>();

        driver.Update(
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            null,
            null,
            true,
            false,
            _fixture.Create<DateOnly>(),
            _fixture.Create<string>(),
            reason);

        var entry = Assert.IsType<DriverAuditEntry>(driver.AuditLog.First());
        Assert.Equal(reason, entry.Reason);
    }
}
