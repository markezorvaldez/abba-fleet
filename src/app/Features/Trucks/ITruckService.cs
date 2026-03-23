using AbbaFleet.Shared;

namespace AbbaFleet.Features.Trucks;

public interface ITruckService
{
    Task<IReadOnlyList<TruckSummary>> GetAllAsync();

    Task<TruckDetailDto?> GetByIdAsync(Guid id);

    Task<Result<Truck>> CreateAsync(UpsertTruckRequest request, bool forceDriverAssignment = false);

    Task<Result<TruckDetailDto>> UpdateAsync(Guid id, UpsertTruckRequest request, bool forceDriverAssignment = false);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<IReadOnlyList<LookupItem>> GetDriverOptionsAsync();

    Task<Result<bool>> DeactivateAsync(Guid id, string reason);

    Task<Result<bool>> ReactivateAsync(Guid id, string reason);

    Task<Result<TruckDetailDto>> AssignDriverAsync(Guid truckId, Guid driverId, string reason, bool force = false);

    Task<Result<TruckDetailDto>> UnassignDriverAsync(Guid truckId, string reason);

    Task<(IReadOnlyList<InvestmentDto> Entries, decimal Total)> GetInvestmentsAsync(Guid truckId);

    Task<Result<InvestmentDto>> AddInvestmentAsync(Guid truckId, AddInvestmentRequest request);
}
