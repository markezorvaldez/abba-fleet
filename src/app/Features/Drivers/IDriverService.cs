using AbbaFleet.Shared;

namespace AbbaFleet.Features.Drivers;

public interface IDriverService
{
    Task<IReadOnlyList<DriverSummary>> GetAllAsync();
    Task<DriverDetailDto?> GetByIdAsync(Guid id);
    Task<Result<Driver>> CreateAsync(UpsertDriverRequest request);
    Task<Result<DriverDetailDto>> UpdateAsync(Guid id, UpsertDriverRequest request);
    Task<Result<bool>> DeleteAsync(Guid id);
}
