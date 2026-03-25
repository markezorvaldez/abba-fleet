using System.Security.Claims;
using AbbaFleet.Features.Files;
using AbbaFleet.Shared;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Features.Files;

public class FileHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IFileService _fileService = Substitute.For<IFileService>();

    private static HttpContext AuthenticatedContext(string userName)
    {
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)])));

        return httpContext;
    }

    private static HttpContext UnauthenticatedContext()
    {
        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        return httpContext;
    }

    private IFormFile FormFile(string fileName = "test.pdf", string contentType = "application/pdf", long size = 1024)
    {
        var formFile = Substitute.For<IFormFile>();
        formFile.FileName.Returns(fileName);
        formFile.ContentType.Returns(contentType);
        formFile.Length.Returns(size);
        formFile.OpenReadStream().Returns(new MemoryStream([1, 2, 3]));

        return formFile;
    }

    // --- DownloadAsync ---

    [Fact]
    public async Task DownloadAsync_FileExists_ReturnsFileResult()
    {
        var id = _fixture.Create<Guid>();
        var stream = new MemoryStream([1, 2, 3]);

        _fileService.DownloadFileAsync(Arg.Is(id))
                    .Returns(((Stream)stream, "application/pdf", "report.pdf"));

        var result = await FileHandlers.DownloadAsync(id, _fileService);

        Assert.IsType<FileStreamHttpResult>(result);
    }

    [Fact]
    public async Task DownloadAsync_FileNotFound_ReturnsNotFound()
    {
        var id = _fixture.Create<Guid>();

        _fileService.DownloadFileAsync(Arg.Is(id))
                    .Returns((ValueTuple<Stream, string, string>?)null);

        var result = await FileHandlers.DownloadAsync(id, _fileService);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task UploadAsync_InvalidEntityId_ReturnsBadRequest()
    {
        var result = await FileHandlers.UploadAsync(
            AuthenticatedContext(_fixture.Create<string>()),
            FormFile(),
            NoteEntityType.Driver.ToString(),
            "not-a-guid",
            _fileService);

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("entity ID", badRequest.Value, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadAsync_InvalidEntityType_ReturnsBadRequest()
    {
        var result = await FileHandlers.UploadAsync(
            AuthenticatedContext(_fixture.Create<string>()),
            FormFile(),
            "NotAValidType",
            _fixture.Create<Guid>().ToString(),
            _fileService);

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("entity type", badRequest.Value, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadAsync_ServiceFailure_ReturnsBadRequest()
    {
        var userName = _fixture.Create<string>();
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var errorMessage = _fixture.Create<string>();

        _fileService.UploadFileAsync(
                        Arg.Any<Guid?>(),
                        Arg.Any<NoteEntityType>(),
                        Arg.Any<Guid>(),
                        Arg.Any<Stream>(),
                        Arg.Any<string>(),
                        Arg.Any<long>(),
                        Arg.Any<string>(),
                        Arg.Any<string>())
                    .Returns((Result<FileDto>)errorMessage);

        var result = await FileHandlers.UploadAsync(
            AuthenticatedContext(userName),
            FormFile(),
            entityType.ToString(),
            entityId.ToString(),
            _fileService);

        var badRequest = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal(errorMessage, badRequest.Value);
    }

    // --- UploadAsync ---

    [Fact]
    public async Task UploadAsync_Unauthenticated_ReturnsUnauthorized()
    {
        var result = await FileHandlers.UploadAsync(
            UnauthenticatedContext(),
            FormFile(),
            NoteEntityType.Driver.ToString(),
            _fixture.Create<Guid>().ToString(),
            _fileService);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task UploadAsync_ValidRequest_ReturnsOkWithDto()
    {
        var userName = _fixture.Create<string>();
        var entityType = NoteEntityType.Truck;
        var entityId = _fixture.Create<Guid>();
        var dto = _fixture.Create<FileDto>();

        _fileService.UploadFileAsync(
                        Arg.Any<Guid?>(),
                        Arg.Is(entityType),
                        Arg.Is(entityId),
                        Arg.Any<Stream>(),
                        Arg.Any<string>(),
                        Arg.Any<long>(),
                        Arg.Any<string>(),
                        Arg.Is(userName))
                    .Returns((Result<FileDto>)dto);

        var result = await FileHandlers.UploadAsync(
            AuthenticatedContext(userName),
            FormFile(),
            entityType.ToString(),
            entityId.ToString(),
            _fileService);

        var ok = Assert.IsType<Ok<FileDto>>(result);
        Assert.Equal(dto, ok.Value);
    }
}
