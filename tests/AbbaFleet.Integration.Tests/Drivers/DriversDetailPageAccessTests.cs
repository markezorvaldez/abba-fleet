using System.Net;
using System.Security.Claims;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Integration.Tests.Fixtures;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AbbaFleet.Integration.Tests.Drivers;

[Collection("Integration")]
public class DriversDetailPageAccessTests(IntegrationTestFixture fixture)
{
    private static readonly Guid TestDriverId = Guid.NewGuid();

    [Fact]
    public async Task DriverDetailPage_UnauthenticatedRequest_RedirectsToLogin()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/drivers/{TestDriverId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task DriverDetailPage_AuthenticatedWithManageDrivers_Returns200()
    {
        var client = fixture.CreateClient();
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = IntegrationTestFixture.AdminEmail;
        formData["Input.Password"] = IntegrationTestFixture.AdminPassword;
        await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        var response = await client.GetAsync($"/drivers/{TestDriverId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DriverDetailPage_AuthenticatedWithoutManageDrivers_RedirectsToDashboard()
    {
        const string email = "nodetailperm@test.com";
        const string password = IntegrationTestFixture.AdminPassword;
        await CreateUserWithoutDriverPermissionAsync(email, password);

        var client = fixture.CreateClient();
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = email;
        formData["Input.Password"] = password;
        await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        var response = await client.GetAsync($"/drivers/{TestDriverId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.AbsolutePath);
    }

    private async Task CreateUserWithoutDriverPermissionAsync(string email, string password)
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
    }
}
