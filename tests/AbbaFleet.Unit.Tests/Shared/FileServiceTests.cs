using System.Security.Claims;
using AbbaFleet.Infrastructure;
using AbbaFleet.Shared;
using AutoFixture;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Shared;

public class FileServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IFileRepository _fileRepository = Substitute.For<IFileRepository>();
    private readonly INoteRepository _noteRepository = Substitute.For<INoteRepository>();
    private readonly IFileStorageService _storageService = Substitute.For<IFileStorageService>();
    private readonly AuthenticationStateProvider _authStateProvider = Substitute.For<AuthenticationStateProvider>();
    private readonly ILogger<FileService> _logger = Substitute.For<ILogger<FileService>>();
    private readonly string _userName;

    public FileServiceTests()
    {
        _userName = _fixture.Create<string>();
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, _userName)],
            "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);
        _authStateProvider.GetAuthenticationStateAsync().Returns(Task.FromResult(authState));
    }

    private FileService CreateService() => new(
        _fileRepository,
        _noteRepository,
        _storageService,
        _authStateProvider,
        _logger);

    private static (Stream Stream, string Name, long Size, string ContentType) CreateTestFile(
        string name = "test.pdf", long size = 1024, string contentType = "application/pdf")
        => (new MemoryStream(), name, size, contentType);

    // --- GetFilesForEntityAsync ---

    [Fact]
    public async Task GetFilesForEntityAsync_ReturnsFilesForEntity()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();

        var file1 = new AttachedFile(null, entityType, entityId, "a.pdf", 100, "application/pdf", "path/a.pdf", _fixture.Create<string>());
        var file2 = new AttachedFile(null, entityType, entityId, "b.pdf", 200, "application/pdf", "path/b.pdf", _fixture.Create<string>());

        _fileRepository.GetByEntityAsync(Arg.Is(entityType), Arg.Is(entityId))
            .Returns(new List<AttachedFile> { file1, file2 });

        var service = CreateService();
        var result = await service.GetFilesForEntityAsync(entityType, entityId);

        Assert.Equal(2, result.Count);
        Assert.Equal("a.pdf", result[0].FileName);
        Assert.Equal("b.pdf", result[1].FileName);
    }

    [Fact]
    public async Task GetFilesForEntityAsync_NoFiles_ReturnsEmptyList()
    {
        var entityType = NoteEntityType.Truck;
        var entityId = _fixture.Create<Guid>();

        _fileRepository.GetByEntityAsync(Arg.Is(entityType), Arg.Is(entityId))
            .Returns(new List<AttachedFile>());

        var service = CreateService();
        var result = await service.GetFilesForEntityAsync(entityType, entityId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilesForEntityAsync_FilesWithNote_IncludesNoteTitle()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var noteId = _fixture.Create<Guid>();
        var noteTitle = _fixture.Create<string>();

        var note = new Note(entityType, entityId, noteTitle, _fixture.Create<string>(), _fixture.Create<string>());
        var file = new AttachedFile(noteId, entityType, entityId, "a.pdf", 100, "application/pdf", "path/a.pdf", _fixture.Create<string>());

        _fileRepository.GetByEntityAsync(Arg.Is(entityType), Arg.Is(entityId))
            .Returns(new List<AttachedFile> { file });
        _noteRepository.GetByIdAsync(Arg.Is(noteId)).Returns(note);

        var service = CreateService();
        var result = await service.GetFilesForEntityAsync(entityType, entityId);

        Assert.Single(result);
        Assert.Equal(noteTitle, result[0].NoteTitle);
    }

    // --- GetFilesForNoteAsync ---

    [Fact]
    public async Task GetFilesForNoteAsync_ReturnsFilesForNote()
    {
        var noteId = _fixture.Create<Guid>();
        var entityId = _fixture.Create<Guid>();
        var noteTitle = _fixture.Create<string>();

        var note = new Note(NoteEntityType.Driver, entityId, noteTitle, _fixture.Create<string>(), _fixture.Create<string>());
        var file = new AttachedFile(noteId, NoteEntityType.Driver, entityId, "doc.pdf", 512, "application/pdf", "path/doc.pdf", _fixture.Create<string>());

        _fileRepository.GetByNoteIdAsync(Arg.Is(noteId)).Returns(new List<AttachedFile> { file });
        _noteRepository.GetByIdAsync(Arg.Is(noteId)).Returns(note);

        var service = CreateService();
        var result = await service.GetFilesForNoteAsync(noteId);

        Assert.Single(result);
        Assert.Equal("doc.pdf", result[0].FileName);
        Assert.Equal(noteTitle, result[0].NoteTitle);
    }

    // --- UploadFileAsync ---

    [Fact]
    public async Task UploadFileAsync_ValidFile_ReturnsSuccessAndSavesToRepository()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var storagePath = $"driver/{entityId}/file.pdf";
        var (stream, name, size, contentType) = CreateTestFile("file.pdf", 512, "application/pdf");

        _storageService
            .SaveAsync(Arg.Any<Stream>(), Arg.Is("file.pdf"), Arg.Is(entityType), Arg.Is(entityId))
            .Returns(storagePath);

        var service = CreateService();
        var result = await service.UploadFileAsync(null, entityType, entityId, stream, name, size, contentType);

        Assert.True(result.Succeeded);
        Assert.Equal("file.pdf", result.Value!.FileName);
        Assert.Equal(_userName, result.Value.UploadedBy);
        await _fileRepository.Received(1).AddAsync(Arg.Is<AttachedFile>(f =>
            f.EntityType == entityType &&
            f.EntityId == entityId &&
            f.FileName == "file.pdf" &&
            f.FileSize == 512 &&
            f.ContentType == "application/pdf" &&
            f.StoragePath == storagePath &&
            f.UploadedBy == _userName));
    }

    [Fact]
    public async Task UploadFileAsync_FileTooLarge_ReturnsFailure()
    {
        var (stream, name, size, contentType) = CreateTestFile("big.pdf", 11 * 1024 * 1024, "application/pdf");

        var service = CreateService();
        var result = await service.UploadFileAsync(null, NoteEntityType.Driver, _fixture.Create<Guid>(), stream, name, size, contentType);

        Assert.False(result.Succeeded);
        Assert.Contains("10 MB", result.Error, StringComparison.OrdinalIgnoreCase);
        await _fileRepository.DidNotReceive().AddAsync(Arg.Any<AttachedFile>());
    }

    [Fact]
    public async Task UploadFileAsync_NoteIdProvidedAndNoteNotFound_ReturnsFailure()
    {
        var noteId = _fixture.Create<Guid>();
        var (stream, name, size, contentType) = CreateTestFile();

        _noteRepository.GetByIdAsync(Arg.Is(noteId)).Returns((Note?)null);

        var service = CreateService();
        var result = await service.UploadFileAsync(noteId, NoteEntityType.Driver, _fixture.Create<Guid>(), stream, name, size, contentType);

        Assert.False(result.Succeeded);
        Assert.Contains("note", result.Error, StringComparison.OrdinalIgnoreCase);
        await _fileRepository.DidNotReceive().AddAsync(Arg.Any<AttachedFile>());
    }

    [Fact]
    public async Task UploadFileAsync_NoteIdProvidedAndNoteExists_Succeeds()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var noteId = _fixture.Create<Guid>();
        var storagePath = $"driver/{entityId}/file.pdf";
        var note = new Note(entityType, entityId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var (stream, name, size, contentType) = CreateTestFile("file.pdf", 512, "application/pdf");

        _noteRepository.GetByIdAsync(Arg.Is(noteId)).Returns(note);
        _storageService
            .SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<NoteEntityType>(), Arg.Any<Guid>())
            .Returns(storagePath);

        var service = CreateService();
        var result = await service.UploadFileAsync(noteId, entityType, entityId, stream, name, size, contentType);

        Assert.True(result.Succeeded);
        Assert.Equal(noteId, result.Value!.NoteId);
    }

    // --- DeleteFileAsync ---

    [Fact]
    public async Task DeleteFileAsync_FileExists_ReturnsSuccessAndDeletesFromRepositoryAndStorage()
    {
        var entityId = _fixture.Create<Guid>();
        var storagePath = "driver/somepath/file.pdf";
        var file = new AttachedFile(
            null, NoteEntityType.Driver, entityId, "file.pdf", 100, "application/pdf", storagePath, _fixture.Create<string>());

        _fileRepository.GetByIdAsync(Arg.Is(file.Id)).Returns(file);

        var service = CreateService();
        var result = await service.DeleteFileAsync(file.Id);

        Assert.True(result.Succeeded);
        await _fileRepository.Received(1).DeleteAsync(Arg.Is<AttachedFile>(f => f.Id == file.Id));
        await _storageService.Received(1).DeleteAsync(Arg.Is(storagePath));
    }

    [Fact]
    public async Task DeleteFileAsync_FileNotFound_ReturnsFailure()
    {
        var fileId = _fixture.Create<Guid>();
        _fileRepository.GetByIdAsync(Arg.Is(fileId)).Returns((AttachedFile?)null);

        var service = CreateService();
        var result = await service.DeleteFileAsync(fileId);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _fileRepository.DidNotReceive().DeleteAsync(Arg.Any<AttachedFile>());
        await _storageService.DidNotReceive().DeleteAsync(Arg.Any<string>());
    }

    // --- DownloadFileAsync ---

    [Fact]
    public async Task DownloadFileAsync_FileExists_ReturnsStreamAndMetadata()
    {
        var entityId = _fixture.Create<Guid>();
        var storagePath = "driver/path/report.pdf";
        var file = new AttachedFile(
            null, NoteEntityType.Driver, entityId, "report.pdf", 1024, "application/pdf", storagePath, _fixture.Create<string>());

        var expectedStream = new MemoryStream();
        _fileRepository.GetByIdAsync(Arg.Is(file.Id)).Returns(file);
        _storageService.OpenRead(Arg.Is(storagePath)).Returns(expectedStream);

        var service = CreateService();
        var result = await service.DownloadFileAsync(file.Id);

        Assert.NotNull(result);
        Assert.Equal(expectedStream, result.Value.stream);
        Assert.Equal("application/pdf", result.Value.contentType);
        Assert.Equal("report.pdf", result.Value.fileName);
    }

    [Fact]
    public async Task DownloadFileAsync_FileNotFound_ReturnsNull()
    {
        var fileId = _fixture.Create<Guid>();
        _fileRepository.GetByIdAsync(Arg.Is(fileId)).Returns((AttachedFile?)null);

        var service = CreateService();
        var result = await service.DownloadFileAsync(fileId);

        Assert.Null(result);
    }
}
