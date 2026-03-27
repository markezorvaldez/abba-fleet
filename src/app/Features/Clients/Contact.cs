namespace AbbaFleet.Features.Clients;

public class Contact
{
    private Contact() { } // EF Core

    public Contact(
        Guid clientId,
        string fullName,
        string? role,
        string? phoneNumber,
        string? email)
    {
        var trimmed = fullName.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmed, nameof(fullName));

        Id = Guid.NewGuid();
        ClientId = clientId;
        FullName = trimmed;
        Role = role?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Email = email?.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? Role { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void Update(string fullName, string? role, string? phoneNumber, string? email)
    {
        var trimmed = fullName.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmed, nameof(fullName));

        FullName = trimmed;
        Role = role?.Trim();
        PhoneNumber = phoneNumber?.Trim();
        Email = email?.Trim();
    }
}
