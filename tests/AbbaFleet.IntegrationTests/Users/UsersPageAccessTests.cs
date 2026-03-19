using System.Net;
using AbbaFleet.IntegrationTests.Fixtures;
using Xunit;

namespace AbbaFleet.IntegrationTests.Users;

[Collection("Integration")]
public class UsersPageAccessTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task UsersPage_UnauthenticatedRequest_RedirectsToLogin()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/users");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString());
    }
}
