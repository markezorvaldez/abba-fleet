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

        if (await userManager.Users.AnyAsync(stoppingToken))
        {
            return;
        }

        var seedEmail = configuration["SEED_ADMIN_EMAIL"];
        var seedPassword = configuration["SEED_ADMIN_PASSWORD"];

        if (seedEmail is null || seedPassword is null)
        {
            logger.LogWarning("No users exist and SEED_ADMIN_EMAIL / SEED_ADMIN_PASSWORD are not set — skipping seed");
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
        user.GrantAll();

        var result = await userManager.CreateAsync(user, seedPassword);
        if (result.Succeeded)
        {
            logger.LogInformation("Seed admin account created: {Email}", seedEmail);
        }
        else
        {
            logger.LogError("Failed to create seed admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
