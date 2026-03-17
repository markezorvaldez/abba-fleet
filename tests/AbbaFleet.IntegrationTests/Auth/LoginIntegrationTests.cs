using System.Net;
using System.Text.RegularExpressions;

namespace AbbaFleet.IntegrationTests.Auth;

[Collection("Integration")]
public class LoginIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task UnauthenticatedRequest_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task LoginPage_Returns200()
    {
        var response = await _client.GetAsync("/account/login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_RedirectsToDashboard()
    {
        var formData = await GetFormFieldsFromLoginPage();
        formData["Input.Email"] = IntegrationTestFixture.AdminEmail;
        formData["Input.Password"] = IntegrationTestFixture.AdminPassword;

        var response = await _client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsLoginPageWithError()
    {
        var formData = await GetFormFieldsFromLoginPage();
        formData["Input.Email"] = IntegrationTestFixture.AdminEmail;
        formData["Input.Password"] = "WrongPassword!";

        var response = await _client.PostAsync("/account/login", new FormUrlEncodedContent(formData));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Invalid email or password", html);
    }

    private async Task<Dictionary<string, string>> GetFormFieldsFromLoginPage()
    {
        var response = await _client.GetAsync("/account/login");
        var html = await response.Content.ReadAsStringAsync();

        var fields = new Dictionary<string, string>();
        var matches = Regex.Matches(html, @"<input\s[^>]*type=[""']hidden[""'][^>]*/?>", RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var name = Regex.Match(match.Value, @"name=[""']([^""']+)[""']").Groups[1].Value;
            var value = Regex.Match(match.Value, @"value=[""']([^""']*)[""']").Groups[1].Value;
            if (!string.IsNullOrEmpty(name))
                fields[name] = value;
        }

        return fields;
    }
}
