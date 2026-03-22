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

        var driver = Driver.Create(
            request.FullName,
            request.PhoneNumber,
            request.FacebookLink,
            request.Address,
            request.IsReliever,
            request.DateStarted);

        await repository.AddAsync(driver);
        return driver;
    }
}
