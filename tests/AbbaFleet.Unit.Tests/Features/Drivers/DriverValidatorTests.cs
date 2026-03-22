using AbbaFleet.Features.Drivers;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Drivers;

public class DriverValidatorTests
{
    private readonly DriverValidator _validator = new();
    private readonly IFixture _fixture = CreateFixture();

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Register(() => DateOnly.FromDateTime(fixture.Create<DateTime>()));
        return fixture;
    }

    [Fact]
    public void ValidInput_Succeeds()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var facebook = _fixture.Create<string>();
        var address = _fixture.Create<string>();
        var date = _fixture.Create<DateOnly>();

        var result = Driver.TryCreate(
            _validator, name, phone, facebook, address,
            isReliever: true, date);

        Assert.True(result.Succeeded);
        Assert.Equal(name, result.Value!.FullName);
        Assert.Equal(phone, result.Value.PhoneNumber);
        Assert.Equal(facebook, result.Value.FacebookLink);
        Assert.Equal(address, result.Value.Address);
        Assert.True(result.Value.IsReliever);
        Assert.True(result.Value.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyFullName_Fails(string fullName)
    {
        var result = Driver.TryCreate(
            _validator,
            fullName, _fixture.Create<string>(),
            null, null, false, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyPhoneNumber_Fails(string phoneNumber)
    {
        var result = Driver.TryCreate(
            _validator,
            _fixture.Create<string>(), phoneNumber,
            null, null, false, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void FullNameExceeds100Chars_Fails()
    {
        var result = Driver.TryCreate(
            _validator,
            new string('A', 101), _fixture.Create<string>(),
            null, null, false, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void PhoneNumberExceeds100Chars_Fails()
    {
        var result = Driver.TryCreate(
            _validator,
            _fixture.Create<string>(), new string('1', 101),
            null, null, false, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void FacebookLinkExceeds100Chars_Fails()
    {
        var result = Driver.TryCreate(
            _validator,
            _fixture.Create<string>(), _fixture.Create<string>(),
            new string('A', 101), null, false, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void AddressExceeds100Chars_Fails()
    {
        var result = Driver.TryCreate(
            _validator,
            _fixture.Create<string>(), _fixture.Create<string>(),
            null, new string('A', 101), false, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void NullOptionalFields_Succeeds()
    {
        var result = Driver.TryCreate(
            _validator,
            _fixture.Create<string>(), _fixture.Create<string>(),
            null, null, false, _fixture.Create<DateOnly>());

        Assert.True(result.Succeeded);
        Assert.Null(result.Value!.FacebookLink);
        Assert.Null(result.Value.Address);
    }

    [Fact]
    public void TrimsWhitespace()
    {
        var name = _fixture.Create<string>();
        var phone = _fixture.Create<string>();
        var facebook = _fixture.Create<string>();
        var address = _fixture.Create<string>();

        var result = Driver.TryCreate(
            _validator,
            $"  {name}  ", $"  {phone}  ",
            $"  {facebook}  ", $"  {address}  ",
            false, _fixture.Create<DateOnly>());

        Assert.True(result.Succeeded);
        Assert.Equal(name, result.Value!.FullName);
        Assert.Equal(phone, result.Value.PhoneNumber);
        Assert.Equal(facebook, result.Value.FacebookLink);
        Assert.Equal(address, result.Value.Address);
    }

    // --- TryUpdate tests ---

    private Driver CreateValidDriver()
    {
        var result = Driver.TryCreate(
            _validator,
            _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<string>(), _fixture.Create<string>(),
            false, _fixture.Create<DateOnly>());
        return result.Value!;
    }

    [Fact]
    public void TryUpdate_ValidInput_UpdatesAllFields()
    {
        var driver = CreateValidDriver();
        var newName = _fixture.Create<string>();
        var newPhone = _fixture.Create<string>();
        var newFacebook = _fixture.Create<string>();
        var newAddress = _fixture.Create<string>();
        var newDate = _fixture.Create<DateOnly>();

        var result = driver.TryUpdate(
            _validator, newName, newPhone, newFacebook, newAddress,
            isActive: false, isReliever: true, newDate);

        Assert.True(result.Succeeded);
        Assert.Equal(newName, driver.FullName);
        Assert.Equal(newPhone, driver.PhoneNumber);
        Assert.Equal(newFacebook, driver.FacebookLink);
        Assert.Equal(newAddress, driver.Address);
        Assert.False(driver.IsActive);
        Assert.True(driver.IsReliever);
        Assert.Equal(newDate, driver.DateStarted);
    }

    [Fact]
    public void TryUpdate_InvalidInput_ReturnsFailure_DoesNotMutate()
    {
        var driver = CreateValidDriver();
        var originalName = driver.FullName;
        var originalPhone = driver.PhoneNumber;
        var originalFacebook = driver.FacebookLink;
        var originalAddress = driver.Address;
        var originalIsActive = driver.IsActive;
        var originalIsReliever = driver.IsReliever;
        var originalDateStarted = driver.DateStarted;

        var result = driver.TryUpdate(
            _validator, "", _fixture.Create<string>(),
            null, null, false, true, _fixture.Create<DateOnly>());

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Equal(originalName, driver.FullName);
        Assert.Equal(originalPhone, driver.PhoneNumber);
        Assert.Equal(originalFacebook, driver.FacebookLink);
        Assert.Equal(originalAddress, driver.Address);
        Assert.Equal(originalIsActive, driver.IsActive);
        Assert.Equal(originalIsReliever, driver.IsReliever);
        Assert.Equal(originalDateStarted, driver.DateStarted);
    }

    [Fact]
    public void TryUpdate_TrimsWhitespace()
    {
        var driver = CreateValidDriver();
        var newName = _fixture.Create<string>();
        var newPhone = _fixture.Create<string>();

        var result = driver.TryUpdate(
            _validator, $"  {newName}  ", $"  {newPhone}  ",
            null, null, true, false, _fixture.Create<DateOnly>());

        Assert.True(result.Succeeded);
        Assert.Equal(newName, driver.FullName);
        Assert.Equal(newPhone, driver.PhoneNumber);
    }

    [Fact]
    public void TryUpdate_SetsUpdatedAt()
    {
        var driver = CreateValidDriver();
        var beforeUpdate = driver.UpdatedAt;

        var result = driver.TryUpdate(
            _validator, _fixture.Create<string>(), _fixture.Create<string>(),
            null, null, true, false, _fixture.Create<DateOnly>());

        Assert.True(result.Succeeded);
        Assert.True(driver.UpdatedAt >= beforeUpdate);
    }
}
