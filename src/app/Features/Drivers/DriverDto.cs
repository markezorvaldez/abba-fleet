namespace AbbaFleet.Features.Drivers;

public sealed record DriverSummary(
    Guid Id,
    string FullName,
    string PhoneNumber,
    bool IsReliever,
    bool IsActive,
    DateOnly DateStarted);

public sealed record DriverDetailDto(
    Guid Id,
    string FullName,
    string PhoneNumber,
    string? FacebookLink,
    string? Address,
    bool IsReliever,
    bool IsActive,
    DateOnly DateStarted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
