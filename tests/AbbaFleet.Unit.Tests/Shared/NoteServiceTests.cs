using System.Security.Claims;
using AbbaFleet.Shared;
using AutoFixture;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AbbaFleet.Unit.Tests.Shared;

public class NoteServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly INoteRepository _repository = Substitute.For<INoteRepository>();
    private readonly AuthenticationStateProvider _authStateProvider = Substitute.For<AuthenticationStateProvider>();
    private readonly ILogger<NoteService> _logger = Substitute.For<ILogger<NoteService>>();
    private readonly string _userName;

    public NoteServiceTests()
    {
        _userName = _fixture.Create<string>();

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, _userName)],
            "TestAuth");

        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);
        _authStateProvider.GetAuthenticationStateAsync().Returns(Task.FromResult(authState));
    }

    private NoteService CreateService()
    {
        return new NoteService(_repository, _authStateProvider, _logger);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateNoteAsync_EmptyBody_ReturnsFailure(string? body)
    {
        var service = CreateService();
        var result = await service.CreateNoteAsync(NoteEntityType.Driver, _fixture.Create<Guid>(), _fixture.Create<string>(), body!);

        Assert.False(result.Succeeded);
        Assert.Contains("Body", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Note>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateNoteAsync_EmptyTitle_ReturnsFailure(string? title)
    {
        var service = CreateService();
        var result = await service.CreateNoteAsync(NoteEntityType.Driver, _fixture.Create<Guid>(), title!, _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("Title", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Note>());
    }

    // --- CreateNoteAsync ---

    [Fact]
    public async Task CreateNoteAsync_ValidInput_ReturnsSuccessAndAddsToRepository()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var title = _fixture.Create<string>();
        var body = _fixture.Create<string>();

        var service = CreateService();
        var result = await service.CreateNoteAsync(entityType, entityId, title, body);

        Assert.True(result.Succeeded);
        Assert.Equal(title, result.Value!.Title);
        Assert.Equal(body, result.Value.Body);
        Assert.Equal(_userName, result.Value.CreatedBy);

        await _repository.Received(1)
                         .AddAsync(
                             Arg.Is<Note>(n =>
                                 n.EntityType == entityType
                                 && n.EntityId == entityId
                                 && n.Title == title
                                 && n.Body == body
                                 && n.CreatedBy == _userName));
    }

    // --- DeleteNoteAsync ---

    [Fact]
    public async Task DeleteNoteAsync_NoteExists_ReturnsSuccessAndCallsRepository()
    {
        var existingNote = new Note(
            NoteEntityType.Truck,
            _fixture.Create<Guid>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        _repository.GetByIdAsync(Arg.Is(existingNote.Id)).Returns(existingNote);

        var service = CreateService();
        var result = await service.DeleteNoteAsync(existingNote.Id);

        Assert.True(result.Succeeded);
        await _repository.Received(1).DeleteAsync(Arg.Is<Note>(n => n.Id == existingNote.Id));
    }

    [Fact]
    public async Task DeleteNoteAsync_NoteNotFound_ReturnsFailure()
    {
        var noteId = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(noteId)).Returns((Note?)null);

        var service = CreateService();
        var result = await service.DeleteNoteAsync(noteId);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Note>());
    }

    [Fact]
    public async Task GetNotesAsync_NoNotes_ReturnsEmptyList()
    {
        var entityType = NoteEntityType.Truck;
        var entityId = _fixture.Create<Guid>();

        _repository.GetByEntityAsync(
                       Arg.Is(entityType),
                       Arg.Is(entityId))
                   .Returns(new List<Note>());

        var service = CreateService();
        var notes = await service.GetNotesAsync(entityType, entityId);

        Assert.Empty(notes);
    }

    // --- GetNotesAsync ---

    [Fact]
    public async Task GetNotesAsync_ReturnsNotesForEntity()
    {
        var entityType = NoteEntityType.Driver;
        var entityId = _fixture.Create<Guid>();
        var note1 = new Note(entityType, entityId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());
        var note2 = new Note(entityType, entityId, _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

        _repository.GetByEntityAsync(
                       Arg.Is(entityType),
                       Arg.Is(entityId))
                   .Returns(
                       new List<Note>
                       {
                           note1,
                           note2
                       });

        var service = CreateService();
        var notes = await service.GetNotesAsync(entityType, entityId);

        Assert.Equal(2, notes.Count);
        Assert.Equal(note1.Title, notes[0].Title);
        Assert.Equal(note2.Title, notes[1].Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task UpdateNoteAsync_EmptyBody_ReturnsFailure(string? body)
    {
        var service = CreateService();
        var result = await service.UpdateNoteAsync(_fixture.Create<Guid>(), _fixture.Create<string>(), body!);

        Assert.False(result.Succeeded);
        Assert.Contains("Body", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Note>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task UpdateNoteAsync_EmptyTitle_ReturnsFailure(string? title)
    {
        var service = CreateService();
        var result = await service.UpdateNoteAsync(_fixture.Create<Guid>(), title!, _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("Title", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Note>());
    }

    [Fact]
    public async Task UpdateNoteAsync_NoteNotFound_ReturnsFailure()
    {
        var noteId = _fixture.Create<Guid>();
        _repository.GetByIdAsync(Arg.Is(noteId)).Returns((Note?)null);

        var service = CreateService();
        var result = await service.UpdateNoteAsync(noteId, _fixture.Create<string>(), _fixture.Create<string>());

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Note>());
    }

    // --- UpdateNoteAsync ---

    [Fact]
    public async Task UpdateNoteAsync_ValidInput_ReturnsSuccessAndCallsRepository()
    {
        var existingNote = new Note(
            NoteEntityType.Driver,
            _fixture.Create<Guid>(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>());

        _repository.GetByIdAsync(Arg.Is(existingNote.Id)).Returns(existingNote);

        var newTitle = _fixture.Create<string>();
        var newBody = _fixture.Create<string>();

        var service = CreateService();
        var result = await service.UpdateNoteAsync(existingNote.Id, newTitle, newBody);

        Assert.True(result.Succeeded);
        Assert.Equal(newTitle, result.Value!.Title);
        Assert.Equal(newBody, result.Value.Body);
        Assert.Equal(_userName, result.Value.ModifiedBy);

        await _repository.Received(1)
                         .UpdateAsync(
                             Arg.Is<Note>(n =>
                                 n.Id == existingNote.Id && n.Title == newTitle && n.Body == newBody && n.ModifiedBy == _userName));
    }
}
