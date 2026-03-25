using AbbaFleet;
using AbbaFleet.Features.Files;
using AbbaFleet.Infrastructure;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Shared;
using FluentValidation;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);

    config.MinimumLevel.Information()
          .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
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
       .AddInteractiveServerComponents()
       .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddDataProtection()
       .PersistKeysToDbContext<AppDbContext>();

builder.Services.AddMudServices();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

builder.Services.AddHostedService<MigrationHostedService>();

builder.Host.UseLamar(registry =>
{
    registry.Scan(scan =>
    {
        scan.AssemblyContainingType<Program>();
        scan.WithDefaultConventions(ServiceLifetime.Scoped);
        scan.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
    });

    registry.For<IFileStorageService>().Use<R2FileStorageService>();
});

var app = builder.Build();

app.Logger.LogInformation("🚀 STARTSUCCESS: Railway Serilog configuration is working!");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/health", () => Results.Ok());

app.MapFileEndpoints();

app.MapGet(
    "/account/logout",
    async (SignInManager<ApplicationUser> signInManager) =>
    {
        await signInManager.SignOutAsync();

        return Results.Redirect(AppRoutes.Login);
    });

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

await app.RunAsync();

public partial class Program
{
}
