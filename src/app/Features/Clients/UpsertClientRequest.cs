namespace AbbaFleet.Features.Clients;

public sealed record UpsertClientRequest(
    string CompanyName,
    string? Description,
    string? Address,
    decimal TaxRate,
    bool IsActive = true);
