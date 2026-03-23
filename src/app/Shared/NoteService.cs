using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace AbbaFleet.Shared;

public class NoteService(
    INoteRepository repository,
    AuthenticationStateProvider authStateProvider,
    ILogger<NoteService> logger) : INoteService
{
    public async Task<IReadOnlyList<NoteDto>> GetNotesAsync(NoteEntityType entityType, Guid entityId)
    {
        var notes = await repository.GetByEntityAsync(entityType, entityId);
        return notes.Select(MapToDto).ToList();
    }

    public async Task<Result<NoteDto>> CreateNoteAsync(
        NoteEntityType entityType, Guid entityId, string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return "Body is required.";
        }

        var userName = await GetCurrentUserNameAsync();

        if (userName is null)
        {
            return "Unable to determine the current user.";
        }

        var note = new Note(entityType, entityId, title, body, userName);
        await repository.AddAsync(note);

        logger.LogInformation(
            "Note {NoteId} created for {EntityType} {EntityId} by {UserName}",
            note.Id, entityType, entityId, userName);

        return MapToDto(note);
    }

    public async Task<Result<NoteDto>> UpdateNoteAsync(Guid noteId, string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return "Body is required.";
        }

        var note = await repository.GetByIdAsync(noteId);

        if (note is null)
        {
            return "Note not found.";
        }

        var userName = await GetCurrentUserNameAsync();

        if (userName is null)
        {
            return "Unable to determine the current user.";
        }

        note.Update(title, body, userName);
        await repository.UpdateAsync(note);

        logger.LogInformation(
            "Note {NoteId} updated for {EntityType} {EntityId} by {UserName}",
            note.Id, note.EntityType, note.EntityId, userName);

        return MapToDto(note);
    }

    public async Task<Result<bool>> DeleteNoteAsync(Guid noteId)
    {
        var note = await repository.GetByIdAsync(noteId);

        if (note is null)
        {
            return "Note not found.";
        }

        var userName = await GetCurrentUserNameAsync();

        if (userName is null)
        {
            return "Unable to determine the current user.";
        }

        logger.LogInformation(
            "Note {NoteId} deleted from {EntityType} {EntityId} by {UserName}",
            note.Id, note.EntityType, note.EntityId, userName);

        await repository.DeleteAsync(note);
        return true;
    }

    private async Task<string?> GetCurrentUserNameAsync()
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirstValue(ClaimTypes.Name);
    }

    private static NoteDto MapToDto(Note n) => new(
        n.Id,
        n.Title,
        n.Body,
        n.CreatedBy,
        n.CreatedAt,
        n.ModifiedBy,
        n.ModifiedAt);
}
