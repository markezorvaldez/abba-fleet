using AbbaFleet;
using AbbaFleet.Infrastructure;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Shared;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;
var configuration = builder.Configuration;

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);

    config.MinimumLevel.Information()
          .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
          .Enrich.FromLogContext();

    // 3. Standardize the output format
    if (context.HostingEnvironment.IsProduction())
    {
        config.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
    }
    else
    {
        config.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    }
});

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = AppRoutes.Login;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

builder.Services.AddMudServices();
builder.Services.AddHostedService<MigrationHostedService>();
builder.Services.AddHostedService<AdminSeedService>();

builder.Host.UseLamar(registry =>
{
    registry.Scan(scan =>
    {
        scan.AssemblyContainingType<Program>();
        scan.WithDefaultConventions(ServiceLifetime.Scoped);
        scan.ConnectImplementationsToTypesClosing(typeof(FluentValidation.IValidator<>));
    });
});

var app = builder.Build();

app.Logger.LogInformation("🚀 STARTSUCCESS: Railway Serilog configuration is working!");


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok());

app.MapGet("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect(AppRoutes.Login);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();

public partial class Program { }
