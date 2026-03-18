using AbbaFleet;
using AbbaFleet.Infrastructure;
using AbbaFleet.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace AbbaFleet.IntegrationTests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    public const string AdminEmail = "test@abbafleet.com";
    public const string AdminPassword = "TestPass1!";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("abbafleet_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
                builder.UseSetting("Seed:AdminEmail", AdminEmail);
                builder.UseSetting("Seed:AdminPassword", AdminPassword);
                builder.ConfigureServices(services =>
                {
                    // Remove the background migration service; fixture runs setup synchronously below
                    var descriptor = services.SingleOrDefault(d => d.ImplementationType == typeof(MigrationHostedService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                });
            });

        // Run migrations and seed the admin user before any tests execute
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(AdminEmail) == null)
        {
            var user = new ApplicationUser { UserName = AdminEmail, Email = AdminEmail, FullName = "Admin", EmailConfirmed = true };
            await userManager.CreateAsync(user, AdminPassword);
        }
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public HttpClient CreateClient() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });
}
