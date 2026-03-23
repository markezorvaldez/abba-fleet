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

// Blazor Server streams IBrowserFile data over SignalR JS interop.
// Default MaximumReceiveMessageSize is 32 KB, which is too small for file uploads.
builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB — matches max upload size
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

builder.Services.AddMudServices();
builder.Services.AddSingleton(TimeProvider.System);
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

app.MapGet("/api/files/{id:guid}", async (Guid id, IFileService fileService) =>
{
    var result = await fileService.DownloadFileAsync(id);

    if (result is null)
    {
        return Results.NotFound();
    }

    var (stream, contentType, fileName) = result.Value;
    return Results.File(stream, contentType, fileName);
}).RequireAuthorization();

app.MapPost("/api/files/upload", async (HttpRequest request, IFileRepository fileRepository, IFileStorageService storageService) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");

    if (file is null)
    {
        return Results.BadRequest("No file provided.");
    }

    var entityTypeStr = form["entityType"].ToString();
    var entityIdStr = form["entityId"].ToString();

    if (!Enum.TryParse<NoteEntityType>(entityTypeStr, out var entityType)
        || !Guid.TryParse(entityIdStr, out var entityId))
    {
        return Results.BadRequest("Invalid entityType or entityId.");
    }

    Guid? noteId = Guid.TryParse(form["noteId"].ToString(), out var nid) ? nid : null;
    var userName = request.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "unknown";

    await using var stream = file.OpenReadStream();
    var storagePath = await storageService.SaveAsync(stream, file.FileName, entityType, entityId);

    var attachedFile = new AttachedFile(noteId, entityType, entityId, file.FileName, file.Length, file.ContentType, storagePath, userName);
    await fileRepository.AddAsync(attachedFile);

    return Results.Ok(new FileDto(attachedFile.Id, attachedFile.NoteId, null, attachedFile.FileName, attachedFile.FileSize, attachedFile.ContentType, attachedFile.UploadedBy, attachedFile.UploadedAt));
}).RequireAuthorization().DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();

public partial class Program { }
