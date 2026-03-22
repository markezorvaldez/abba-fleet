namespace AbbaFleet.Shared;

public interface INoteRepository
{
    Task<IReadOnlyList<Note>> GetByEntityAsync(NoteEntityType entityType, Guid entityId);
    Task<Note?> GetByIdAsync(Guid id);
    Task AddAsync(Note note);
    Task UpdateAsync(Note note);
    Task DeleteAsync(Note note);
}
