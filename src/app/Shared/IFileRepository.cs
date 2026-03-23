namespace AbbaFleet.Shared;

public interface IFileRepository
{
    Task<IReadOnlyList<AttachedFile>> GetByEntityAsync(NoteEntityType entityType, Guid entityId);

    Task<IReadOnlyList<AttachedFile>> GetByNoteIdAsync(Guid noteId);

    Task<AttachedFile?> GetByIdAsync(Guid id);

    Task AddAsync(AttachedFile file);

    Task DeleteAsync(AttachedFile file);
}
