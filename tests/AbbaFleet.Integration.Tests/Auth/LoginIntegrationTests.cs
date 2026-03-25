using System.Net;
using AbbaFleet.Integration.Tests.Fixtures;
using Xunit;

namespace AbbaFleet.Integration.Tests.Auth;

[Collection("Integration")]
public class LoginIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task LoginPage_Returns200()
    {
        var response = await _client.GetAsync("/account/login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsLoginPageWithError()
    {
        var formData = await fixture.GetLoginFormFieldsAsync(_client);
        formData["Input.Email"] = IntegrationTestFixture.AdminEmail;
        formData["Input.Password"] = "WrongPassword!";

        var response = await _client.PostAsync("/account/login", new FormUrlEncodedContent(formData));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Invalid email or password", html);
    }

    [Fact]
    public async Task Login_WithValidCredentials_RedirectsToDashboard()
    {
        var formData = await fixture.GetLoginFormFieldsAsync(_client);
        formData["Input.Email"] = IntegrationTestFixture.AdminEmail;
        formData["Input.Password"] = IntegrationTestFixture.AdminPassword;

        var response = await _client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task UnauthenticatedRequest_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString());
    }
}
