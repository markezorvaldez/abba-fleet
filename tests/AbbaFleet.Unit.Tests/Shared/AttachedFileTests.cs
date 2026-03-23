using AbbaFleet.Shared;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Shared;

public class AttachedFileTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var fileName = _fixture.Create<string>();
        var fileSize = _fixture.Create<long>() % 1_000_000 + 1;
        var contentType = _fixture.Create<string>();
        var storagePath = _fixture.Create<string>();
        var uploadedBy = _fixture.Create<string>();

        var file = new AttachedFile(null, entityType, entityId, fileName, fileSize, contentType, storagePath, uploadedBy);

        Assert.NotEqual(Guid.Empty, file.Id);
        Assert.Null(file.NoteId);
        Assert.Equal(entityType, file.EntityType);
        Assert.Equal(entityId, file.EntityId);
        Assert.Equal(fileName, file.FileName);
        Assert.Equal(fileSize, file.FileSize);
        Assert.Equal(contentType, file.ContentType);
        Assert.Equal(storagePath, file.StoragePath);
        Assert.Equal(uploadedBy, file.UploadedBy);
        Assert.True(file.UploadedAt > DateTimeOffset.MinValue);
    }

    [Fact]
    public void Constructor_WithNoteId_SetsNoteId()
    {
        var noteId = _fixture.Create<Guid>();

        var file = new AttachedFile(
            noteId,
            NoteEntityType.Truck,
            _fixture.Create<Guid>(),
            _fixture.Create<string>(),
            100,
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        Assert.Equal(noteId, file.NoteId);
    }

    [Fact]
    public void Constructor_EmptyEntityId_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new AttachedFile(
                null,
                NoteEntityType.Driver,
                Guid.Empty,
                _fixture.Create<string>(),
                100,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>()));
    }

    [Fact]
    public void Constructor_ZeroFileSize_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new AttachedFile(
                null,
                NoteEntityType.Driver,
                _fixture.Create<Guid>(),
                _fixture.Create<string>(),
                0,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>()));
    }

    [Fact]
    public void Constructor_NegativeFileSize_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new AttachedFile(
                null,
                NoteEntityType.Driver,
                _fixture.Create<Guid>(),
                _fixture.Create<string>(),
                -1,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyFileName_Throws(string? fileName)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new AttachedFile(
                null,
                NoteEntityType.Driver,
                _fixture.Create<Guid>(),
                fileName!,
                100,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyStoragePath_Throws(string? storagePath)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new AttachedFile(
                null,
                NoteEntityType.Driver,
                _fixture.Create<Guid>(),
                _fixture.Create<string>(),
                100,
                _fixture.Create<string>(),
                storagePath!,
                _fixture.Create<string>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyUploadedBy_Throws(string? uploadedBy)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new AttachedFile(
                null,
                NoteEntityType.Driver,
                _fixture.Create<Guid>(),
                _fixture.Create<string>(),
                100,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                uploadedBy!));
    }
}
