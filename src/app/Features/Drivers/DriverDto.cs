namespace AbbaFleet.Features.Drivers;

public sealed record DriverSummary(
    Guid Id,
    string FullName,
    string PhoneNumber,
    bool IsReliever,
    bool IsActive,
    DateOnly DateStarted);
