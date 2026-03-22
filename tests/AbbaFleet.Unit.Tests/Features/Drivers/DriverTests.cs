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

        var driver = Driver.Create(
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

        var driver = Driver.Create(name, phone, null, null, true, date);

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
            Driver.Create(fullName, _fixture.Create<string>(), null, null, false, _fixture.Create<DateOnly>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ThrowsOnEmptyPhoneNumber(string phoneNumber)
    {
        Assert.Throws<ArgumentException>(() =>
            Driver.Create(_fixture.Create<string>(), phoneNumber, null, null, false, _fixture.Create<DateOnly>()));
    }
}
