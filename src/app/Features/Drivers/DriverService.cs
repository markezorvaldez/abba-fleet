using AbbaFleet.Shared;
using FluentValidation;

namespace AbbaFleet.Features.Drivers;

public class DriverService(IValidator<UpsertDriverRequest> validator, IDriverRepository repository) : IDriverService
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

    public async Task<Result<Driver>> CreateAsync(UpsertDriverRequest request)
    {
        var validation = validator.Validate(request);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        var driver = new Driver(
            request.FullName,
            request.PhoneNumber,
            request.FacebookLink,
            request.Address,
            request.IsReliever,
            request.DateStarted);

        await repository.AddAsync(driver);
        return driver;
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

    public async Task<Result<DriverDetailDto>> UpdateAsync(Guid id, UpsertDriverRequest request)
    {
        var validation = validator.Validate(request);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        var driver = await repository.GetByIdAsync(id);

        if (driver is null)
        {
            return "Driver not found.";
        }

        driver.Update(
            request.FullName,
            request.PhoneNumber,
            request.FacebookLink,
            request.Address,
            request.IsActive,
            request.IsReliever,
            request.DateStarted);

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
