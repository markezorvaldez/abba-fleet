using AbbaFleet.Shared;
using AutoFixture;
using Xunit;

namespace AbbaFleet.Unit.Tests.Shared;

public class NoteTests
{
    private readonly IFixture _fixture = new Fixture();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyBody_Throws(string? body)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new Note(NoteEntityType.Driver, _fixture.Create<Guid>(), _fixture.Create<string>(), body!, _fixture.Create<string>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyCreatedBy_Throws(string? createdBy)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new Note(NoteEntityType.Driver, _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<string>(), createdBy!));
    }

    [Fact]
    public void Constructor_EmptyEntityId_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new Note(NoteEntityType.Driver, Guid.Empty, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyTitle_Throws(string? title)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new Note(NoteEntityType.Driver, _fixture.Create<Guid>(), title!, _fixture.Create<string>(), _fixture.Create<string>()));
    }

    [Fact]
    public void Constructor_TrimsInputs()
    {
        var title = "  my title  ";
        var body = "  my body  ";
        var entityId = _fixture.Create<Guid>();

        var note = new Note(NoteEntityType.Truck, entityId, title, body, _fixture.Create<string>());

        Assert.Equal("my title", note.Title);
        Assert.Equal("my body", note.Body);
    }

    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var title = _fixture.Create<string>();
        var body = _fixture.Create<string>();
        var createdBy = _fixture.Create<string>();

        var note = new Note(entityType, entityId, title, body, createdBy);

        Assert.NotEqual(Guid.Empty, note.Id);
        Assert.Equal(entityType, note.EntityType);
        Assert.Equal(entityId, note.EntityId);
        Assert.Equal(title, note.Title);
        Assert.Equal(body, note.Body);
        Assert.Equal(createdBy, note.CreatedBy);
        Assert.True(note.CreatedAt > DateTimeOffset.MinValue);
        Assert.Null(note.ModifiedBy);
        Assert.Null(note.ModifiedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_EmptyBody_Throws(string? body)
    {
        var note = new Note(
            NoteEntityType.Driver,
            _fixture.Create<Guid>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        Assert.ThrowsAny<ArgumentException>(() =>
            note.Update(_fixture.Create<string>(), body!, _fixture.Create<string>()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_EmptyTitle_Throws(string? title)
    {
        var note = new Note(
            NoteEntityType.Driver,
            _fixture.Create<Guid>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        Assert.ThrowsAny<ArgumentException>(() =>
            note.Update(title!, _fixture.Create<string>(), _fixture.Create<string>()));
    }

    [Fact]
    public void Update_ValidInput_SetsProperties()
    {
        var note = new Note(
            NoteEntityType.Driver,
            _fixture.Create<Guid>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        var newTitle = _fixture.Create<string>();
        var newBody = _fixture.Create<string>();
        var modifiedBy = _fixture.Create<string>();

        note.Update(newTitle, newBody, modifiedBy);

        Assert.Equal(newTitle, note.Title);
        Assert.Equal(newBody, note.Body);
        Assert.Equal(modifiedBy, note.ModifiedBy);
        Assert.NotNull(note.ModifiedAt);
    }
}
