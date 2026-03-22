using AbbaFleet.Shared;
using FluentValidation;

namespace AbbaFleet.Features.Drivers;

public class Driver
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string? FacebookLink { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsReliever { get; private set; }
    public DateOnly DateStarted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Driver() { } // EF Core

    public static Result<Driver> TryCreate(
        IValidator<Driver> validator,
        string fullName,
        string phoneNumber,
        string? facebookLink,
        string? address,
        bool isReliever,
        DateOnly dateStarted)
    {
        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            FacebookLink = facebookLink?.Trim(),
            Address = address?.Trim(),
            IsReliever = isReliever,
            DateStarted = dateStarted,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var validation = validator.Validate(driver);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        return driver;
    }

    public Result<Driver> TryUpdate(
        IValidator<Driver> validator,
        string fullName,
        string phoneNumber,
        string? facebookLink,
        string? address,
        bool isActive,
        bool isReliever,
        DateOnly dateStarted)
    {
        var candidate = new Driver
        {
            FullName = fullName.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            FacebookLink = facebookLink?.Trim(),
            Address = address?.Trim(),
            IsActive = isActive,
            IsReliever = isReliever,
            DateStarted = dateStarted
        };

        var validation = validator.Validate(candidate);

        if (!validation.IsValid)
        {
            return string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage));
        }

        FullName = candidate.FullName;
        PhoneNumber = candidate.PhoneNumber;
        FacebookLink = candidate.FacebookLink;
        Address = candidate.Address;
        IsActive = candidate.IsActive;
        IsReliever = candidate.IsReliever;
        DateStarted = candidate.DateStarted;
        UpdatedAt = DateTimeOffset.UtcNow;
        return this;
    }
}
