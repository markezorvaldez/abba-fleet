using System.Security.Claims;
using AbbaFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure;

public class MigrationHostedService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<MigrationHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await db.Database.MigrateAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed — app will run without DB access");
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var seedEmail = configuration["SEED_ADMIN_EMAIL"];
        var seedPassword = configuration["SEED_ADMIN_PASSWORD"];

        if (seedEmail is null || seedPassword is null)
        {
            logger.LogWarning("SEED_ADMIN_EMAIL / SEED_ADMIN_PASSWORD are not set — skipping seed");
            return;
        }

        var seedAdmin = await userManager.FindByEmailAsync(seedEmail);

        if (seedAdmin is not null)
        {
            // Upgrade path: grant all permissions if the seed admin was created before permissions were introduced
            var existingClaims = await userManager.GetClaimsAsync(seedAdmin);
            var hasPermissions = existingClaims.Any(c => c.Type == PermissionClaimTypes.Permission);
            if (!hasPermissions)
            {
                await GrantAllPermissionsAsync(userManager, seedAdmin);
                logger.LogInformation("Granted all permissions to seed admin: {Email}", seedEmail);
            }

            return;
        }

        if (await userManager.Users.AnyAsync(stoppingToken))
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = seedEmail,
            Email = seedEmail,
            FullName = "Admin",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, seedPassword);
        if (result.Succeeded)
        {
            await GrantAllPermissionsAsync(userManager, user);
            logger.LogInformation("Seed admin account created: {Email}", seedEmail);
        }
        else
        {
            logger.LogError("Failed to create seed admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task GrantAllPermissionsAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        foreach (var p in Enum.GetValues<Permission>())
        {
            await userManager.AddClaimAsync(user, new Claim(PermissionClaimTypes.Permission, p.ToString()));
        }
    }
}
