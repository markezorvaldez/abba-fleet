using AbbaFleet.Shared;

namespace AbbaFleet.Features.Trucks;

public interface ITruckRepository
{
    Task<IReadOnlyList<(Truck Truck, string? DriverName)>> GetAllAsync();
    Task<(Truck Truck, string? DriverName)?> GetByIdAsync(Guid id);
    Task AddAsync(Truck truck);
    Task UpdateAsync(Truck truck);
    Task DeleteAsync(Truck truck);
    Task<bool> ExistsWithPlateNumberAsync(string plateNumber, Guid? excludeId = null);
    Task<IReadOnlyList<LookupItem>> GetActiveDriverOptionsAsync();
    Task<(Truck Truck, string? DriverName)?> GetByDriverIdAsync(Guid driverId);
    Task<DriverLookup?> GetDriverLookupAsync(Guid driverId);
}
