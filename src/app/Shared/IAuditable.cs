namespace AbbaFleet.Shared;

public interface IAuditable
{
    IReadOnlyCollection<AuditEntryBase> AuditLog { get; }

    void ClearAuditLog();
}
