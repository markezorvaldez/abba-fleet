namespace AbbaFleet.Shared;

public class Note
{
    public Guid Id { get; private set; }
    public NoteEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public string? ModifiedBy { get; private set; }
    public DateTimeOffset? ModifiedAt { get; private set; }

    private Note() { } // EF Core

    public Note(NoteEntityType entityType, Guid entityId, string title, string body, string createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(body, nameof(body));
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy, nameof(createdBy));

        var trimmedTitle = title.Trim();
        var trimmedBody = body.Trim();

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity ID must not be empty.", nameof(entityId));
        }

        Id = Guid.NewGuid();
        EntityType = entityType;
        EntityId = entityId;
        Title = trimmedTitle;
        Body = trimmedBody;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string title, string body, string modifiedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(body, nameof(body));
        ArgumentException.ThrowIfNullOrWhiteSpace(modifiedBy, nameof(modifiedBy));

        Title = title.Trim();
        Body = body.Trim();
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTimeOffset.UtcNow;
    }
}
