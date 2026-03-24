using AbbaFleet.Shared;
using FluentValidation;

namespace AbbaFleet.Features.Drivers;

public class DriverService(
    IValidator<UpsertDriverRequest> validator,
    IDriverRepository repository,
    IFileRepository fileRepository,
    IFileStorageService fileStorageService) : IDriverService
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
        var validation = await validator.ValidateAsync(request);

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

        return driver is null ? null : MapToDetail(driver);
    }

    public async Task<Result<DriverDetailDto>> UpdateAsync(Guid id, UpsertDriverRequest request)
    {
        var validation = await validator.ValidateAsync(request);

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

        var files = await fileRepository.GetByEntityAsync(NoteEntityType.Driver, id);

        foreach (var file in files)
        {
            await fileStorageService.DeleteAsync(file.StoragePath);
            await fileRepository.DeleteAsync(file);
        }

        await repository.DeleteAsync(driver);

        return true;
    }

    public async Task<Result<bool>> DeactivateAsync(Guid id, string reason)
    {
        var driver = await repository.GetByIdAsync(id);

        if (driver is null)
        {
            return "Driver not found.";
        }

        if (!driver.IsActive)
        {
            return "Driver is already inactive.";
        }

        driver.Update(
            driver.FullName,
            driver.PhoneNumber,
            driver.FacebookLink,
            driver.Address,
            isActive: false,
            driver.IsReliever,
            driver.DateStarted);

        await repository.UpdateAsync(driver);

        return true;
    }

    public async Task<Result<bool>> ReactivateAsync(Guid id)
    {
        var driver = await repository.GetByIdAsync(id);

        if (driver is null)
        {
            return "Driver not found.";
        }

        if (driver.IsActive)
        {
            return "Driver is already active.";
        }

        driver.Update(
            driver.FullName,
            driver.PhoneNumber,
            driver.FacebookLink,
            driver.Address,
            isActive: true,
            driver.IsReliever,
            driver.DateStarted);

        await repository.UpdateAsync(driver);

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
