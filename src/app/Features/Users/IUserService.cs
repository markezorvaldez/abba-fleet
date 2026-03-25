using AbbaFleet.Shared;

namespace AbbaFleet.Features.Users;

public interface IUserService
{
    Task<IReadOnlyList<UserSummary>> GetAllAsync();

    Task<UserResult> CreateAsync(string fullName, string email, string password, IReadOnlySet<Permission> permissions);

    Task<UserResult> UpdateAsync(
        string userId,
        string fullName,
        bool isActive,
        string? newPassword,
        IReadOnlySet<Permission> targetPermissions,
        string currentUserId);

    Task<UserResult> DeleteAsync(string userId);

    Task<UserResult> ToggleActiveAsync(string userId, bool makeActive);
}
