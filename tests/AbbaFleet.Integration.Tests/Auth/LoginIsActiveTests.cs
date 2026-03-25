using System.Net;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AbbaFleet.Integration.Tests.Auth;

[Collection("Integration")]
public class LoginIsActiveTests(IntegrationTestFixture fixture)
{
    private async Task CreateUserAsync(string email, bool isActive)
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
            IsActive = isActive
        };

        await userManager.CreateAsync(user, IntegrationTestFixture.AdminPassword);
    }

    [Fact]
    public async Task Login_WithDeactivatedUser_ShowsDeactivatedMessage()
    {
        await CreateUserAsync("deactivated@test.com", isActive: false);
        var client = fixture.CreateClient();
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = "deactivated@test.com";
        formData["Input.Password"] = IntegrationTestFixture.AdminPassword;

        var response = await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("deactivated", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithValidCredentials_SetsLastLoginAt()
    {
        await CreateUserAsync("lastlogin@test.com", isActive: true);
        var client = fixture.CreateClient();
        var formData = await fixture.GetLoginFormFieldsAsync(client);
        formData["Input.Email"] = "lastlogin@test.com";
        formData["Input.Password"] = IntegrationTestFixture.AdminPassword;

        var before = DateTimeOffset.UtcNow;
        await client.PostAsync("/account/login", new FormUrlEncodedContent(formData));

        using var scope = fixture.Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync("lastlogin@test.com");

        Assert.NotNull(user!.LastLoginAt);
        Assert.True(user.LastLoginAt >= before);
    }
}
