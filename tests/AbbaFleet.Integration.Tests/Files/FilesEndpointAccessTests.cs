using System.Net;
using AbbaFleet.Integration.Tests.Fixtures;
using Xunit;

namespace AbbaFleet.Integration.Tests.Files;

[Collection("Integration")]
public class FilesEndpointAccessTests(IntegrationTestFixture fixture)
{
    private static readonly Guid TestFileId = Guid.NewGuid();

    [Fact]
    public async Task DownloadEndpoint_UnauthenticatedRequest_RedirectsToLogin()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/api/files/{TestFileId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task UploadEndpoint_UnauthenticatedRequest_RedirectsToLogin()
    {
        var client = fixture.CreateClient();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Driver"), "entityType");
        content.Add(new StringContent(Guid.NewGuid().ToString()), "entityId");
        content.Add(new ByteArrayContent([]), "file");

        var response = await client.PostAsync("/api/files/upload", content);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/login", response.Headers.Location?.ToString());
    }
}
