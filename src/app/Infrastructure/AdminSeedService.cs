using System.Security.Claims;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure;

public class AdminSeedService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<AdminSeedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var seedEmail = configuration["SEED_ADMIN_EMAIL"];
        var seedPassword = configuration["SEED_ADMIN_PASSWORD"];

        if (seedEmail is null || seedPassword is null)
        {
            logger.LogWarning("SEED_ADMIN_EMAIL / SEED_ADMIN_PASSWORD are not set — skipping admin seed");
            return;
        }

        var user = await userManager.FindByEmailAsync(seedEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = seedEmail,
                Email = seedEmail,
                FullName = "Admin",
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, seedPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create seed admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Seed admin account created: {Email}", seedEmail);
        }

        await EnsureAllPermissionsAsync(userManager, user);
    }

    private async Task EnsureAllPermissionsAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        var existingClaims = await userManager.GetClaimsAsync(user);
        var existingPermissions = existingClaims
            .Where(c => c.Type == PermissionClaimTypes.Permission)
            .Select(c => c.Value)
            .ToHashSet();

        var added = 0;
        foreach (var p in Enum.GetValues<Permission>())
        {
            if (!existingPermissions.Contains(p.ToString()))
            {
                await userManager.AddClaimAsync(user, new Claim(PermissionClaimTypes.Permission, p.ToString()));
                added++;
            }
        }

        if (added > 0)
        {
            logger.LogInformation("Added {Count} missing permissions to seed admin {Email}", added, user.Email);
        }
    }
}
