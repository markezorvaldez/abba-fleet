using AbbaFleet.Features.Clients;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Clients;

public class ClientValidatorTests
{
    private readonly ClientValidator _validator = new();

    private static Fixture CreateFixture()
    {
        return new Fixture();
    }

    [Fact]
    public async Task CompanyNameExceeds100Chars_FailsValidation()
    {
        var request = new UpsertClientRequest(new string('A', 101), null, null, 0m, true);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpsertClientRequest.CompanyName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task EmptyCompanyName_FailsValidation(string name)
    {
        var fixture = CreateFixture();

        var request = fixture.Build<UpsertClientRequest>()
                             .With(r => r.CompanyName, name)
                             .Create();

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpsertClientRequest.CompanyName));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public async Task TaxRateOutOfRange_FailsValidation(double rate)
    {
        var request = new UpsertClientRequest("ACME", null, null, (decimal)rate, true);

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpsertClientRequest.TaxRate));
    }

    [Fact]
    public async Task ValidRequest_PassesValidation()
    {
        var fixture = CreateFixture();

        var request = fixture.Build<UpsertClientRequest>()
                             .With(r => r.CompanyName, "ACME Corp")
                             .With(r => r.TaxRate, 12.5m)
                             .Create();

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
