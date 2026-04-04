using AbbaFleet.Shared;

namespace AbbaFleet.Features.Drivers;

public class Driver : IAuditable
{
    private readonly List<AuditEntryBase> _auditLog = [];

    private Driver() { } // EF Core

    public Driver(
        string fullName,
        string phoneNumber,
        string? facebookLink,
        string? address,
        bool isReliever,
        DateOnly dateStarted,
        string changedBy)
    {
        var trimmedName = fullName.Trim();
        var trimmedPhone = phoneNumber.Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedName, nameof(fullName));
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedPhone, nameof(phoneNumber));

        Id = Guid.NewGuid();
        FullName = trimmedName;
        PhoneNumber = trimmedPhone;
        FacebookLink = facebookLink?.Trim();
        Address = address?.Trim();
        IsReliever = isReliever;
        DateStarted = dateStarted;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;

        _auditLog.Add(new DriverAuditEntry(Id, AuditActionType.Created, changedBy));
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? FacebookLink { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsReliever { get; private set; }
    public DateOnly DateStarted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<AuditEntryBase> AuditLog => _auditLog.AsReadOnly();

    public void ClearAuditLog()
    {
        _auditLog.Clear();
    }

    public void Update(
        string fullName,
        string phoneNumber,
        string? facebookLink,
        string? address,
        bool isActive,
        bool isReliever,
        DateOnly dateStarted,
        string changedBy,
        string? reason = null)
    {
        var trimmedName = fullName.Trim();
        var trimmedPhone = phoneNumber.Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedName, nameof(fullName));
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedPhone, nameof(phoneNumber));

        FullName = trimmedName;
        PhoneNumber = trimmedPhone;
        FacebookLink = facebookLink?.Trim();
        Address = address?.Trim();
        IsActive = isActive;
        IsReliever = isReliever;
        DateStarted = dateStarted;
        UpdatedAt = DateTimeOffset.UtcNow;

        _auditLog.Add(new DriverAuditEntry(Id, AuditActionType.Updated, changedBy, reason));
    }

    public void Deactivate(string changedBy, string? reason = null)
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;

        _auditLog.Add(new DriverAuditEntry(Id, AuditActionType.Deactivated, changedBy, reason));
    }

    public void Reactivate(string changedBy, string? reason = null)
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;

        _auditLog.Add(new DriverAuditEntry(Id, AuditActionType.Reactivated, changedBy, reason));
    }
}
