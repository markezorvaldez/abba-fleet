namespace AbbaFleet.Features.Drivers;

public sealed record UpsertDriverRequest(
    string FullName,
    string PhoneNumber,
    string? FacebookLink,
    string? Address,
    bool IsReliever,
    DateOnly DateStarted,
    bool IsActive = true);
