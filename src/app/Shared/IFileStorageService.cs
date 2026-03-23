namespace AbbaFleet.Shared;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream stream, string fileName, NoteEntityType entityType, Guid entityId);

    Task DeleteAsync(string storagePath);

    Task<Stream?> OpenReadAsync(string storagePath);
}
