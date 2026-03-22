using AbbaFleet.Shared;

namespace AbbaFleet.Features.Trucks;

public interface ITruckService
{
    Task<IReadOnlyList<TruckSummary>> GetAllAsync();
    Task<TruckDetailDto?> GetByIdAsync(Guid id);
    Task<Result<Truck>> CreateAsync(UpsertTruckRequest request);
    Task<Result<TruckDetailDto>> UpdateAsync(Guid id, UpsertTruckRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<IReadOnlyList<LookupItem>> GetDriverOptionsAsync();
}
