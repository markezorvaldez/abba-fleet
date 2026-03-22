namespace AbbaFleet.Features.Trucks;

public sealed record UpsertTruckRequest(
    string PlateNumber,
    string TruckModel,
    OwnershipType OwnershipType,
    Guid? DriverId,
    DateOnly DateAcquired,
    bool IsActive = true);
