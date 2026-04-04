namespace AbbaFleet.Shared;

public abstract class AuditEntryBase
{
    protected AuditEntryBase() { } // EF Core

    protected AuditEntryBase(
        AuditActionType actionType,
        string changedBy,
        string? reason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(changedBy, nameof(changedBy));

        Id = Guid.NewGuid();
        ActionType = actionType;
        ChangedBy = changedBy;
        Reason = reason;
        Timestamp = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; protected set; }
    public AuditActionType ActionType { get; protected set; }
    public string ChangedBy { get; protected set; } = string.Empty;
    public string? Reason { get; protected set; }
    public DateTimeOffset Timestamp { get; protected set; }
}
