namespace AbbaFleet.Features.Files;

public static class FileEndpoints
{
    public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/files/{id:guid}", FileHandlers.DownloadAsync)
           .RequireAuthorization();

        app.MapPost("/api/files/upload", FileHandlers.UploadAsync)
           .RequireAuthorization()
           .DisableAntiforgery();

        return app;
    }
}
