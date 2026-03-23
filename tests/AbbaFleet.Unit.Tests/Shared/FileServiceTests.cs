using AbbaFleet.Infrastructure;
using AbbaFleet.Shared;
using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Shared;

public class FileServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IFileRepository _repository = Substitute.For<IFileRepository>();
    private readonly IFileStorageService _storageService = Substitute.For<IFileStorageService>();
    private readonly ILogger<FileService> _logger = Substitute.For<ILogger<FileService>>();

    private FileService CreateService() => new(_repository, _storageService, _logger);

    // --- UploadFileAsync ---

    [Fact]
    public async Task UploadFileAsync_ValidInput_SavesAndReturnsDto()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var fileName = _fixture.Create<string>();
        var fileSize = 1024L;
        var contentType = _fixture.Create<string>();
        var uploadedBy = _fixture.Create<string>();
        var storagePath = _fixture.Create<string>();
        using var stream = new MemoryStream([1, 2, 3]);

        _storageService.SaveAsync(
                           Arg.Is(stream),
                           Arg.Is(fileName),
                           Arg.Is(entityType),
                           Arg.Is(entityId))
                       .Returns(storagePath);

        var service = CreateService();
        var result = await service.UploadFileAsync(null, entityType, entityId, stream, fileName, fileSize, contentType, uploadedBy);

        Assert.True(result.Succeeded);
        Assert.Equal(fileName, result.Value!.FileName);
        Assert.Equal(fileSize, result.Value.FileSize);
        Assert.Equal(uploadedBy, result.Value.UploadedBy);

        await _repository.Received(1)
                         .AddAsync(
                             Arg.Is<AttachedFile>(f =>
                                 f.EntityType == entityType
                                 && f.EntityId == entityId
                                 && f.FileName == fileName
                                 && f.StoragePath == storagePath
                                 && f.UploadedBy == uploadedBy));
    }

    // --- DeleteFileAsync ---

    [Fact]
    public async Task DeleteFileAsync_FileExists_DeletesFromStorageAndRepository()
    {
        var file = new AttachedFile(
            null,
            NoteEntityType.Truck,
            _fixture.Create<Guid>(),
            _fixture.Create<string>(),
            100,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        _repository.GetByIdAsync(Arg.Is(file.Id)).Returns(file);

        var service = CreateService();
        var result = await service.DeleteFileAsync(file.Id);

        Assert.True(result.Succeeded);
        await _storageService.Received(1).DeleteAsync(Arg.Is(file.StoragePath));
        await _repository.Received(1).DeleteAsync(Arg.Is<AttachedFile>(f => f.Id == file.Id));
    }

    [Fact]
    public async Task DeleteFileAsync_FileNotFound_ReturnsFailure()
    {
        var fileId = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(fileId)).Returns((AttachedFile?)null);

        var service = CreateService();
        var result = await service.DeleteFileAsync(fileId);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _storageService.DidNotReceive().DeleteAsync(Arg.Any<string>());
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<AttachedFile>());
    }

    // --- DownloadFileAsync ---

    [Fact]
    public async Task DownloadFileAsync_FileExists_ReturnsStream()
    {
        var file = new AttachedFile(
            null,
            NoteEntityType.Driver,
            _fixture.Create<Guid>(),
            "report.pdf",
            2048,
            "application/pdf",
            _fixture.Create<string>(),
            _fixture.Create<string>());

        _repository.GetByIdAsync(Arg.Is(file.Id)).Returns(file);

        var stream = new MemoryStream([1, 2, 3]);
        _storageService.OpenReadAsync(Arg.Is(file.StoragePath)).Returns(stream);

        var service = CreateService();
        var result = await service.DownloadFileAsync(file.Id);

        Assert.NotNull(result);
        Assert.Equal("application/pdf", result!.Value.contentType);
        Assert.Equal("report.pdf", result.Value.fileName);
    }

    [Fact]
    public async Task DownloadFileAsync_FileNotFound_ReturnsNull()
    {
        var fileId = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(fileId)).Returns((AttachedFile?)null);

        var service = CreateService();
        var result = await service.DownloadFileAsync(fileId);

        Assert.Null(result);
    }

    [Fact]
    public async Task DownloadFileAsync_R2Orphan_DeletesDbRecordAndReturnsNull()
    {
        var file = new AttachedFile(
            null,
            NoteEntityType.Truck,
            _fixture.Create<Guid>(),
            _fixture.Create<string>(),
            100,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        _repository.GetByIdAsync(Arg.Is(file.Id)).Returns(file);
        _storageService.OpenReadAsync(Arg.Is(file.StoragePath)).Returns((Stream?)null);

        var service = CreateService();
        var result = await service.DownloadFileAsync(file.Id);

        Assert.Null(result);
        await _repository.Received(1).DeleteAsync(Arg.Is<AttachedFile>(f => f.Id == file.Id));
    }

    // --- GetFilesForEntityAsync ---

    [Fact]
    public async Task GetFilesForEntityAsync_ReturnsFilesForEntity()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var uploadedBy = _fixture.Create<string>();

        var file1 = new AttachedFile(
            null,
            entityType,
            entityId,
            _fixture.Create<string>(),
            100,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            uploadedBy);

        var file2 = new AttachedFile(
            null,
            entityType,
            entityId,
            _fixture.Create<string>(),
            200,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            uploadedBy);

        _repository.GetByEntityAsync(Arg.Is(entityType), Arg.Is(entityId))
                   .Returns(
                       new List<AttachedFile>
                       {
                           file1,
                           file2
                       });

        var service = CreateService();
        var files = await service.GetFilesForEntityAsync(entityType, entityId);

        Assert.Equal(2, files.Count);
    }

    [Fact]
    public async Task GetFilesForEntityAsync_NoFiles_ReturnsEmptyList()
    {
        var entityType = NoteEntityType.Truck;
        var entityId = _fixture.Create<Guid>();

        _repository.GetByEntityAsync(Arg.Is(entityType), Arg.Is(entityId))
                   .Returns(new List<AttachedFile>());

        var service = CreateService();
        var files = await service.GetFilesForEntityAsync(entityType, entityId);

        Assert.Empty(files);
    }
}
