using AbbaFleet.Shared;

namespace AbbaFleet.Features.Drivers;

public interface IDriverService
{
    Task<IReadOnlyList<DriverSummary>> GetAllAsync();
    Task<Result<Driver>> CreateAsync(UpsertDriverRequest request);
}
