using AbbaFleet.Shared;

namespace AbbaFleet.Features.Drivers;

public interface IDriverService
{
    Task<IReadOnlyList<DriverSummary>> GetAllAsync();
    Task<Result<Driver>> CreateAsync(string fullName, string phoneNumber, string? facebookLink, string? address, bool isReliever, DateOnly dateStarted);
}
