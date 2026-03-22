namespace AbbaFleet.Features.Trucks;

public enum InvestmentType
{
    Purchase,
    Repair,
    Upgrade,
    Other,
}

public class InvestmentEntry
{
    public Guid Id { get; private set; }
    public Guid TruckId { get; private set; }
    public InvestmentType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public string? Description { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private InvestmentEntry() { } // EF Core

    public InvestmentEntry(
        Guid truckId,
        InvestmentType type,
        decimal amount,
        DateOnly date,
        string? description,
        string createdBy)
    {
        if (truckId == Guid.Empty)
        {
            throw new ArgumentException("TruckId must not be empty.", nameof(truckId));
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new ArgumentException("CreatedBy must not be empty.", nameof(createdBy));
        }

        Id = Guid.NewGuid();
        TruckId = truckId;
        Type = type;
        Amount = amount;
        Date = date;
        Description = description?.Trim();
        CreatedBy = createdBy.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
