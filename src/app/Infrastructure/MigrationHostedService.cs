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
        var seedEmail = configuration["Seed:AdminEmail"];
        var seedPassword = configuration["Seed:AdminPassword"];

        if (seedEmail != null && seedPassword != null)
        {
            var existing = await userManager.FindByEmailAsync(seedEmail);
            if (existing == null)
            {
                var user = new ApplicationUser
                {
                    UserName = seedEmail,
                    Email = seedEmail,
                    FullName = "Admin",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, seedPassword);
            }
        }
    }
}
