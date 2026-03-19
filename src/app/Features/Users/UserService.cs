using System.Security.Claims;
using AbbaFleet.Infrastructure;
using AbbaFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Features.Users;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    public async Task<IReadOnlyList<UserSummary>> GetAllAsync()
    {
        var users = await userManager.Users.OrderBy(u => u.FullName).ToListAsync<ApplicationUser>();
        var summaries = new List<UserSummary>(users.Count);
        foreach (var u in users)
        {
            var claims = await userManager.GetClaimsAsync(u);
            var permissions = claims
                .Where(c => c.Type == PermissionClaimTypes.Permission)
                .Select(c => Enum.TryParse<Permission>(c.Value, out var p) ? (Permission?)p : null)
                .Where(p => p.HasValue)
                .Select(p => p!.Value)
                .ToHashSet();
            summaries.Add(new UserSummary(u.Id, u.FullName, u.Email ?? string.Empty, u.IsActive, u.LastLoginAt, permissions));
        }

        return summaries;
    }

    public async Task<UserResult> CreateAsync(string fullName, string email, string password, IReadOnlySet<Permission> permissions)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return new UserResult(false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        foreach (var p in permissions)
        {
            await userManager.AddClaimAsync(user, new Claim(PermissionClaimTypes.Permission, p.ToString()));
        }

        return new UserResult(true);
    }

    public async Task<UserResult> UpdateAsync(string userId, string fullName, bool isActive, string? newPassword, IReadOnlySet<Permission> targetPermissions, string currentUserId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new UserResult(false, "User not found.");
        }

        user.FullName = fullName;
        user.IsActive = isActive;

        // Self-protection: cannot remove own ManageUsers permission
        var effective = targetPermissions.ToHashSet();
        if (userId == currentUserId)
        {
            effective.Add(Permission.ManageUsers);
        }

        var existingClaims = await userManager.GetClaimsAsync(user);
        var existing = existingClaims
            .Where(c => c.Type == PermissionClaimTypes.Permission)
            .Select(c => c.Value)
            .ToHashSet();
        var target = effective.Select(p => p.ToString()).ToHashSet();

        foreach (var name in target.Except(existing))
        {
            await userManager.AddClaimAsync(user, new Claim(PermissionClaimTypes.Permission, name));
        }

        foreach (var name in existing.Except(target))
        {
            await userManager.RemoveClaimAsync(user, new Claim(PermissionClaimTypes.Permission, name));
        }

        if (!string.IsNullOrEmpty(newPassword))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (!resetResult.Succeeded)
            {
                return new UserResult(false, string.Join(", ", resetResult.Errors.Select(e => e.Description)));
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        return updateResult.Succeeded
            ? new UserResult(true)
            : new UserResult(false, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
    }

    public async Task<UserResult> DeleteAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new UserResult(false, "User not found.");
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded
            ? new UserResult(true)
            : new UserResult(false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<UserResult> ToggleActiveAsync(string userId, bool makeActive)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new UserResult(false, "User not found.");
        }

        if (!makeActive)
        {
            // Block if this is the last active admin
            var adminUsers = await userManager.GetUsersForClaimAsync(
                new Claim(PermissionClaimTypes.Permission, Permission.ManageUsers.ToString()));
            var activeAdminCount = adminUsers.Count(u => u.IsActive);
            var isAdmin = adminUsers.Any(u => u.Id == userId && u.IsActive);

            if (isAdmin && activeAdminCount <= 1)
            {
                return new UserResult(false, "Cannot deactivate the last admin with Manage Users permission.");
            }
        }

        user.IsActive = makeActive;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? new UserResult(true)
            : new UserResult(false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
