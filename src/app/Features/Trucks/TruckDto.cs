namespace AbbaFleet.Features.Trucks;

public sealed record TruckSummary(
    Guid Id,
    string PlateNumber,
    string TruckModel,
    OwnershipType OwnershipType,
    string? DriverName,
    bool IsActive);

public sealed record TruckDetailDto(
    Guid Id,
    string PlateNumber,
    string TruckModel,
    OwnershipType OwnershipType,
    Guid? DriverId,
    string? DriverName,
    bool IsActive,
    DateOnly DateAcquired,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
