using System.Security.Claims;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AbbaFleet.Infrastructure;

public class PermissionService(
    IServiceProvider serviceProvider,
    AuthenticationStateProvider authStateProvider,
    TimeProvider timeProvider) : IPermissionService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private Task<IReadOnlySet<Permission>>? _loadTask;
    private DateTimeOffset? _loadedAt;

    public async Task<bool> HasAsync(Permission permission)
    {
        if (_loadedAt is not null && timeProvider.GetUtcNow() - _loadedAt.Value > CacheTtl)
        {
            _loadTask = null;
        }

        if (_loadTask is null)
        {
            _loadedAt = timeProvider.GetUtcNow();
            _loadTask = LoadPermissionsAsync();
        }

        var permissions = await _loadTask;
        return permissions.Contains(permission);
    }

    private async Task<IReadOnlySet<Permission>> LoadPermissionsAsync()
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        var userId = state.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return new HashSet<Permission>();
        }

        // Create a dedicated scope so concurrent callers don't share a DbContext instance
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        if (user is not { IsActive: true })
        {
            return new HashSet<Permission>();
        }

        var claims = await userManager.GetClaimsAsync(user);
        return claims
            .Where(c => c.Type == PermissionClaimTypes.Permission)
            .Select(c => Enum.TryParse<Permission>(c.Value, out var p) ? (Permission?)p : null)
            .Where(p => p.HasValue)
            .Select(p => p!.Value)
            .ToHashSet();
    }
}
