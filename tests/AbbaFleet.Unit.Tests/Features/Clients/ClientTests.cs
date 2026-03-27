using AbbaFleet.Features.Clients;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Clients;

public class ClientTests
{
    private static Fixture CreateFixture()
    {
        return new Fixture();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_BlankCompanyName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new Client(name, null, null, 0m));
    }

    [Fact]
    public void Constructor_TrimsCompanyName()
    {
        var client = new Client("  ACME Corp  ", null, null, 0m);
        Assert.Equal("ACME Corp", client.CompanyName);
    }

    [Fact]
    public void Constructor_ValidArguments_SetsProperties()
    {
        var fixture = CreateFixture();
        var companyName = fixture.Create<string>();
        var description = fixture.Create<string>();
        var address = fixture.Create<string>();
        const decimal taxRate = 12.5m;

        var client = new Client(companyName, description, address, taxRate);

        Assert.Equal(companyName.Trim(), client.CompanyName);
        Assert.Equal(description, client.Description);
        Assert.Equal(address, client.Address);
        Assert.Equal(taxRate, client.TaxRate);
        Assert.True(client.IsActive);
        Assert.NotEqual(Guid.Empty, client.Id);
        Assert.True(client.CreatedAt > DateTimeOffset.MinValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Update_BlankCompanyName_Throws(string name)
    {
        var client = new Client("Valid Co", null, null, 0m);
        Assert.Throws<ArgumentException>(() => client.Update(name, null, null, 0m, true));
    }

    [Fact]
    public void Update_ChangesAllFields()
    {
        var client = new Client("Original Co", null, null, 0m);
        var before = client.UpdatedAt;

        client.Update("Updated Co", "A description", "123 Main St", 15m, false);

        Assert.Equal("Updated Co", client.CompanyName);
        Assert.Equal("A description", client.Description);
        Assert.Equal("123 Main St", client.Address);
        Assert.Equal(15m, client.TaxRate);
        Assert.False(client.IsActive);
        Assert.True(client.UpdatedAt >= before);
    }
}
