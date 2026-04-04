using AbbaFleet.Shared;

namespace AbbaFleet.Features.Drivers;

public class DriverAuditEntry : AuditEntryBase
{
    private DriverAuditEntry() { } // EF Core

    public DriverAuditEntry(
        Guid driverId,
        AuditActionType actionType,
        string changedBy,
        string? reason = null)
        : base(actionType, changedBy, reason)
    {
        if (driverId == Guid.Empty)
        {
            throw new ArgumentException("Driver ID must not be empty.", nameof(driverId));
        }

        DriverId = driverId;
    }

    public Guid DriverId { get; private set; }
}
