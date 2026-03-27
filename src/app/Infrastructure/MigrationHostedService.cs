using System.Security.Claims;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AbbaFleet.Infrastructure;

public class MigrationHostedService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<MigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await db.Database.MigrateAsync(cancellationToken);

            // Reload Npgsql type map — the citext extension may have just been created by migration
            var conn = (NpgsqlConnection)db.Database.GetDbConnection();
            await conn.OpenAsync(cancellationToken);
            await conn.ReloadTypesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed — app will run without DB access");

            return;
        }

        await SeedAdminAsync(scope.ServiceProvider);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task SeedAdminAsync(IServiceProvider services)
    {
        var seedEmail = configuration["SEED_ADMIN_EMAIL"];
        var seedPassword = configuration["SEED_ADMIN_PASSWORD"];

        if (seedEmail is null || seedPassword is null)
        {
            logger.LogWarning("SEED_ADMIN_EMAIL / SEED_ADMIN_PASSWORD are not set — skipping admin seed");

            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
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
