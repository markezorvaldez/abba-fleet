using System.Net;
using AbbaFleet.Integration.Tests.Fixtures;
using Xunit;

namespace AbbaFleet.Integration.Tests.Clients;

[Collection("Integration")]
public class ClientsDetailPageAccessTests(IntegrationTestFixture fixture)
{
    private static readonly Guid TestClientId = Guid.NewGuid();

    [Fact]
    public async Task ClientDetailPage_AuthenticatedWithManageCompanies_Returns200OrNotFound()
    {
        var client = fixture.CreateClient();
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = IntegrationTestFixture.AdminEmail;
        formData["Input.Password"] = IntegrationTestFixture.AdminPassword;
        await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        var response = await client.GetAsync($"/clients/{TestClientId}");

        // 200 = page loaded (client not found message shown inline), not a redirect
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ClientDetailPage_Unauthenticated_RedirectsToLogin()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/clients/{TestClientId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString() ?? string.Empty);
    }
}
