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
}
