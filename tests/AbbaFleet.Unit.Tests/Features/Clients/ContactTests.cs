using AbbaFleet.Features.Clients;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Clients;

public class ContactTests
{
    private static Fixture CreateFixture()
    {
        return new Fixture();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_BlankFullName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new Contact(Guid.NewGuid(), name, null, null, null));
    }

    [Fact]
    public void Constructor_ValidArguments_SetsProperties()
    {
        var fixture = CreateFixture();
        var clientId = fixture.Create<Guid>();
        var fullName = fixture.Create<string>();
        const string role = "Finance Manager";
        const string phone = "555-1234";
        const string email = "test@example.com";

        var contact = new Contact(clientId, fullName, role, phone, email);

        Assert.Equal(clientId, contact.ClientId);
        Assert.Equal(fullName.Trim(), contact.FullName);
        Assert.Equal(role, contact.Role);
        Assert.Equal(phone, contact.PhoneNumber);
        Assert.Equal(email, contact.Email);
        Assert.NotEqual(Guid.Empty, contact.Id);
    }

    [Fact]
    public void Update_ChangesFields()
    {
        var contact = new Contact(Guid.NewGuid(), "Original Name", null, null, null);

        contact.Update("New Name", "CEO", "555-9999", "new@example.com");

        Assert.Equal("New Name", contact.FullName);
        Assert.Equal("CEO", contact.Role);
        Assert.Equal("555-9999", contact.PhoneNumber);
        Assert.Equal("new@example.com", contact.Email);
    }
}
