using System.Net;
using System.Security.Claims;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Integration.Tests.Fixtures;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AbbaFleet.Integration.Tests.Trucks;

[Collection("Integration")]
public class TrucksPageAccessTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task TrucksPage_UnauthenticatedRequest_RedirectsToLogin()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/trucks");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task TrucksPage_AuthenticatedWithManageTrucks_Returns200()
    {
        var client = fixture.CreateClient();
        await LoginAsAsync(client, IntegrationTestFixture.AdminEmail, IntegrationTestFixture.AdminPassword);

        var response = await client.GetAsync("/trucks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TrucksPage_AuthenticatedWithoutManageTrucks_RedirectsToDashboard()
    {
        const string email = "notruckperm@test.com";
        const string password = IntegrationTestFixture.AdminPassword;
        await CreateUserWithPermissionsAsync(email, password, permissions: []);

        var client = fixture.CreateClient();
        await LoginAsAsync(client, email, password);

        var response = await client.GetAsync("/trucks");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.AbsolutePath);
    }

    private async Task LoginAsAsync(HttpClient client, string email, string password)
    {
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = email;
        formData["Input.Password"] = password;
        await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));
    }

    private async Task CreateUserWithPermissionsAsync(string email, string password, IEnumerable<Permission> permissions)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Test User",
            EmailConfirmed = true,
            IsActive = true
        };
        await userManager.CreateAsync(user, password);

        foreach (var p in permissions)
        {
            await userManager.AddClaimAsync(user, new Claim(PermissionClaimTypes.Permission, p.ToString()));
        }
    }
}
