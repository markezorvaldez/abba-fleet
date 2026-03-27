namespace AbbaFleet.Features.Clients;

public class Client
{
    private Client() { } // EF Core

    public Client(
        string companyName,
        string? description,
        string? address,
        decimal taxRate)
    {
        var trimmed = companyName.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmed, nameof(companyName));

        Id = Guid.NewGuid();
        CompanyName = trimmed;
        Description = description?.Trim();
        Address = address?.Trim();
        TaxRate = taxRate;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Address { get; private set; }
    public decimal TaxRate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Update(
        string companyName,
        string? description,
        string? address,
        decimal taxRate,
        bool isActive)
    {
        var trimmed = companyName.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmed, nameof(companyName));

        CompanyName = trimmed;
        Description = description?.Trim();
        Address = address?.Trim();
        TaxRate = taxRate;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
