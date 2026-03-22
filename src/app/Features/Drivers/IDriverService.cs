using AbbaFleet.Shared;

namespace AbbaFleet.Features.Drivers;

public interface IDriverService
{
    Task<IReadOnlyList<DriverSummary>> GetAllAsync();
    Task<DriverDetailDto?> GetByIdAsync(Guid id);
    Task<Result<Driver>> CreateAsync(string fullName, string phoneNumber, string? facebookLink, string? address, bool isReliever, DateOnly dateStarted);
    Task<Result<DriverDetailDto>> UpdateAsync(Guid id, string fullName, string phoneNumber, string? facebookLink, string? address, bool isActive, bool isReliever, DateOnly dateStarted);
    Task<Result<bool>> DeleteAsync(Guid id);
}
