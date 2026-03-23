namespace AbbaFleet.Shared;

public interface INoteService
{
    Task<IReadOnlyList<NoteDto>> GetNotesAsync(NoteEntityType entityType, Guid entityId);
    Task<Result<NoteDto>> CreateNoteAsync(NoteEntityType entityType, Guid entityId, string title, string body);
    Task<Result<NoteDto>> UpdateNoteAsync(Guid noteId, string title, string body);
    Task<Result<bool>> DeleteNoteAsync(Guid noteId);
}
