using AbbaFleet.Features.Drivers;
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

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var facebook = _fixture.Create<string>();
        var address = _fixture.Create<string>();

        var driver = new Driver(
            $"  {name}  ", $"  {phone}  ",
            $"  {facebook}  ", $"  {address}  ",
            false, _fixture.Create<DateOnly>());

        Assert.Equal(name, driver.FullName);
        Assert.Equal(phone, driver.PhoneNumber);
        Assert.Equal(facebook, driver.FacebookLink);
        Assert.Equal(address, driver.Address);
    }

    [Fact]
    public void Create_SetsExpectedDefaults()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();

        var driver = new Driver(name, phone, null, null, true, date);

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
            new Driver(fullName, _fixture.Create<string>(), null, null, false, _fixture.Create<DateOnly>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnEmptyPhoneNumber(string phoneNumber)
    {
        Assert.Throws<ArgumentException>(() =>
            new Driver(_fixture.Create<string>(), phoneNumber, null, null, false, _fixture.Create<DateOnly>()));
    }

    // --- Update ---

    private Driver CreateDriver() =>
        new Driver(
            _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<string>(), _fixture.Create<string>(),
            false, _fixture.Create<DateOnly>());

    [Fact]
    public void Update_SetsAllFields()
    {
        var driver = CreateDriver();
        var newName = _fixture.Create<string>();
        var newPhone = _fixture.Create<string>();
        var newFacebook = _fixture.Create<string>();
        var newAddress = _fixture.Create<string>();
        var newDate = _fixture.Create<DateOnly>();

        driver.Update(newName, newPhone, newFacebook, newAddress,
            isActive: false, isReliever: true, newDate);

        Assert.Equal(newName, driver.FullName);
        Assert.Equal(newPhone, driver.PhoneNumber);
        Assert.Equal(newFacebook, driver.FacebookLink);
        Assert.Equal(newAddress, driver.Address);
        Assert.False(driver.IsActive);
        Assert.True(driver.IsReliever);
        Assert.Equal(newDate, driver.DateStarted);
    }

    [Fact]
    public void Update_TrimsWhitespace()
    {
        var driver = CreateDriver();
        var newName = _fixture.Create<string>();
        var newPhone = _fixture.Create<string>();

        driver.Update($"  {newName}  ", $"  {newPhone}  ",
            null, null, true, false, _fixture.Create<DateOnly>());

        Assert.Equal(newName, driver.FullName);
        Assert.Equal(newPhone, driver.PhoneNumber);
    }

    [Fact]
    public void Update_SetsUpdatedAt()
    {
        var driver = CreateDriver();
        var beforeUpdate = driver.UpdatedAt;

        driver.Update(_fixture.Create<string>(), _fixture.Create<string>(),
            null, null, true, false, _fixture.Create<DateOnly>());

        Assert.True(driver.UpdatedAt >= beforeUpdate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ThrowsOnEmptyFullName(string fullName)
    {
        var driver = CreateDriver();

        Assert.Throws<ArgumentException>(() =>
            driver.Update(fullName, _fixture.Create<string>(), null, null, true, false, _fixture.Create<DateOnly>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ThrowsOnEmptyPhoneNumber(string phoneNumber)
    {
        var driver = CreateDriver();

        Assert.Throws<ArgumentException>(() =>
            driver.Update(_fixture.Create<string>(), phoneNumber, null, null, true, false, _fixture.Create<DateOnly>()));
    }
}
