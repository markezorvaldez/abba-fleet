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
}
