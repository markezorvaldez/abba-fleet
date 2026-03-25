using System.Net;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AbbaFleet.Integration.Tests.Trucks;

[Collection("Integration")]
public class TrucksDetailPageAccessTests(IntegrationTestFixture fixture)
{
    private static readonly Guid TestTruckId = Guid.NewGuid();

    private async Task CreateUserWithoutTruckPermissionAsync(string email, string password)
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

    [Fact]
    public async Task TruckDetailPage_AuthenticatedWithManageTrucks_Returns200()
    {
        var client = fixture.CreateClient();
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = IntegrationTestFixture.AdminEmail;
        formData["Input.Password"] = IntegrationTestFixture.AdminPassword;
        await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        var response = await client.GetAsync($"/trucks/{TestTruckId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TruckDetailPage_AuthenticatedWithoutManageTrucks_RedirectsToDashboard()
    {
        const string email = "notruckdetailperm@test.com";
        const string password = IntegrationTestFixture.AdminPassword;
        await CreateUserWithoutTruckPermissionAsync(email, password);

        var client = fixture.CreateClient();
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = email;
        formData["Input.Password"] = password;
        await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        var response = await client.GetAsync($"/trucks/{TestTruckId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task TruckDetailPage_UnauthenticatedRequest_RedirectsToLogin()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/trucks/{TestTruckId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString());
    }
}
