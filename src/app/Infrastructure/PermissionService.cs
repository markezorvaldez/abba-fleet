using System.Security.Claims;
using AbbaFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AbbaFleet.Infrastructure;

public class PermissionService(
    IServiceProvider serviceProvider,
    AuthenticationStateProvider authStateProvider) : IPermissionService
{
    private Task<IReadOnlySet<Permission>>? _loadTask;

    public async Task<bool> HasAsync(Permission permission)
    {
        _loadTask ??= LoadPermissionsAsync();
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
