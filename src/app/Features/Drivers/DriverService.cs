using AbbaFleet.Shared;
using FluentValidation;

namespace AbbaFleet.Features.Drivers;

public class DriverService(IValidator<Driver> validator, IDriverRepository repository) : IDriverService
{
    public async Task<IReadOnlyList<DriverSummary>> GetAllAsync()
    {
        var drivers = await repository.GetAllAsync();
        return drivers
            .Select(d => new DriverSummary(
                d.Id,
                d.FullName,
                d.PhoneNumber,
                d.IsReliever,
                d.IsActive,
                d.DateStarted))
            .ToList();
    }

    public async Task<Result<Driver>> CreateAsync(
        string fullName,
        string phoneNumber,
        string? facebookLink,
        string? address,
        bool isReliever,
        DateOnly dateStarted)
    {
        var result = Driver.TryCreate(validator, fullName, phoneNumber, facebookLink, address, isReliever, dateStarted);

        if (!result.Succeeded)
        {
            return result;
        }

        await repository.AddAsync(result.Value!);
        return result;
    }

    public async Task<DriverDetailDto?> GetByIdAsync(Guid id)
    {
        var driver = await repository.GetByIdAsync(id);

        if (driver is null)
        {
            return null;
        }

        return MapToDetail(driver);
    }

    public async Task<Result<DriverDetailDto>> UpdateAsync(
        Guid id,
        string fullName,
        string phoneNumber,
        string? facebookLink,
        string? address,
        bool isActive,
        bool isReliever,
        DateOnly dateStarted)
    {
        var driver = await repository.GetByIdAsync(id);

        if (driver is null)
        {
            return "Driver not found.";
        }

        var result = driver.TryUpdate(validator, fullName, phoneNumber, facebookLink, address, isActive, isReliever, dateStarted);

        if (!result.Succeeded)
        {
            return result.Error!;
        }

        await repository.UpdateAsync(driver);
        return MapToDetail(driver);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var driver = await repository.GetByIdAsync(id);

        if (driver is null)
        {
            return "Driver not found.";
        }

        // TODO: When Trip/Truck entities exist, check for associated records before allowing delete.

        await repository.DeleteAsync(driver);
        return true;
    }

    private static DriverDetailDto MapToDetail(Driver d) => new(
        d.Id,
        d.FullName,
        d.PhoneNumber,
        d.FacebookLink,
        d.Address,
        d.IsReliever,
        d.IsActive,
        d.DateStarted,
        d.CreatedAt,
        d.UpdatedAt);
}
