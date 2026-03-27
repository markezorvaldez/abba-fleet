namespace AbbaFleet.Features.Clients;

public sealed record ClientSummary(
    Guid Id,
    string CompanyName,
    string? Address,
    decimal TaxRate,
    bool IsActive);

public sealed record ClientDetailDto(
    Guid Id,
    string CompanyName,
    string? Description,
    string? Address,
    decimal TaxRate,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ClientAssignedTruckDto> AssignedTrucks);

public sealed record ClientAssignedTruckDto(
    Guid Id,
    string PlateNumber,
    string TruckModel,
    string OwnershipType,
    string? DriverName,
    bool IsActive);

public sealed record ContactDto(
    Guid Id,
    Guid ClientId,
    string FullName,
    string? Role,
    string? PhoneNumber,
    string? Email);

public sealed record UpsertContactRequest(
    string FullName,
    string? Role,
    string? PhoneNumber,
    string? Email);
