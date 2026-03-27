using System.Net;
using AbbaFleet.Integration.Tests.Fixtures;
using Xunit;

namespace AbbaFleet.Integration.Tests.Clients;

[Collection("Integration")]
public class ClientsPageAccessTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task ClientsPage_AuthenticatedWithManageCompanies_Returns200()
    {
        var client = fixture.CreateClient();
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = IntegrationTestFixture.AdminEmail;
        formData["Input.Password"] = IntegrationTestFixture.AdminPassword;
        await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        var response = await client.GetAsync("/clients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ClientsPage_Unauthenticated_RedirectsToLogin()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/clients");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString() ?? string.Empty);
    }
}
