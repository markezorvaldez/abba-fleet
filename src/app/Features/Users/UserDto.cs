using AbbaFleet.Infrastructure;

namespace AbbaFleet.Features.Users;

public sealed record UserSummary(
    string Id,
    string FullName,
    string Email,
    bool IsActive,
    DateTimeOffset? LastLoginAt,
    IReadOnlySet<Permission> Permissions);

public sealed record UserResult(bool Succeeded, string? Error = null);
