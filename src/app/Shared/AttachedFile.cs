namespace AbbaFleet.Shared;

public class AttachedFile
{
    private AttachedFile() { } // EF Core

    public AttachedFile(
        Guid? noteId,
        NoteEntityType entityType,
        Guid entityId,
        string fileName,
        long fileSize,
        string contentType,
        string storagePath,
        string uploadedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(uploadedBy);

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity ID must not be empty.", nameof(entityId));
        }

        if (fileSize <= 0)
        {
            throw new ArgumentException("File size must be greater than zero.", nameof(fileSize));
        }

        Id = Guid.NewGuid();
        NoteId = noteId;
        EntityType = entityType;
        EntityId = entityId;
        FileName = fileName;
        FileSize = fileSize;
        ContentType = contentType;
        StoragePath = storagePath;
        UploadedBy = uploadedBy;
        UploadedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid? NoteId { get; private set; }
    public NoteEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string UploadedBy { get; private set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; private set; }
}
