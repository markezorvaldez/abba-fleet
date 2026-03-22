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
        var request = _fixture.Create<UpsertDriverRequest>();

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyFullName_Fails(string fullName)
    {
        var request = _fixture.Create<UpsertDriverRequest>() with { FullName = fullName };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyPhoneNumber_Fails(string phoneNumber)
    {
        var request = _fixture.Create<UpsertDriverRequest>() with { PhoneNumber = phoneNumber };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void FullNameExceeds100Chars_Fails()
    {
        var request = _fixture.Create<UpsertDriverRequest>() with { FullName = new string('A', 101) };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void PhoneNumberExceeds100Chars_Fails()
    {
        var request = _fixture.Create<UpsertDriverRequest>() with { PhoneNumber = new string('1', 101) };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void FacebookLinkExceeds100Chars_Fails()
    {
        var request = _fixture.Create<UpsertDriverRequest>() with { FacebookLink = new string('A', 101) };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void AddressExceeds100Chars_Fails()
    {
        var request = _fixture.Create<UpsertDriverRequest>() with { Address = new string('A', 101) };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void NullOptionalFields_Succeeds()
    {
        var request = _fixture.Create<UpsertDriverRequest>() with
        {
            FacebookLink = null,
            Address = null
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
