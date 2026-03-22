namespace AbbaFleet.Features.Trucks;

public class Truck
{
    public Guid Id { get; private set; }
    public string PlateNumber { get; private set; } = string.Empty;
    public string TruckModel { get; private set; } = string.Empty;
    public OwnershipType OwnershipType { get; private set; }
    public Guid? DriverId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateOnly DateAcquired { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Truck() { } // EF Core

    public Truck(
        string plateNumber,
        string truckModel,
        OwnershipType ownershipType,
        Guid? driverId,
        DateOnly dateAcquired)
    {
        var trimmedPlate = plateNumber.Trim();
        var trimmedModel = truckModel.Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedPlate, nameof(plateNumber));
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedModel, nameof(truckModel));

        Id = Guid.NewGuid();
        PlateNumber = trimmedPlate;
        TruckModel = trimmedModel;
        OwnershipType = ownershipType;
        DriverId = driverId;
        IsActive = true;
        DateAcquired = dateAcquired;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        string plateNumber,
        string truckModel,
        OwnershipType ownershipType,
        Guid? driverId,
        bool isActive,
        DateOnly dateAcquired)
    {
        var trimmedPlate = plateNumber.Trim();
        var trimmedModel = truckModel.Trim();

        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedPlate, nameof(plateNumber));
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmedModel, nameof(truckModel));

        PlateNumber = trimmedPlate;
        TruckModel = trimmedModel;
        OwnershipType = ownershipType;
        DriverId = driverId;
        IsActive = isActive;
        DateAcquired = dateAcquired;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
