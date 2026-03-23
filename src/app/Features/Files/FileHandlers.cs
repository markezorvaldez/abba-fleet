using System.Security.Claims;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AbbaFleet.Features.Files;

public static class FileHandlers
{
    public static async Task<IResult> DownloadAsync(Guid id, IFileService fileService)
    {
        var result = await fileService.DownloadFileAsync(id);

        if (result is null)
        {
            return Results.NotFound();
        }

        var (stream, contentType, fileName) = result.Value;

        return Results.File(stream, contentType, fileName);
    }

    public static async Task<IResult> UploadAsync(
        HttpContext httpContext,
        IFormFile file,
        [FromForm] string entityType,
        [FromForm] string entityId,
        IFileService fileService)
    {
        var userName = httpContext.User.FindFirstValue(ClaimTypes.Name);

        if (userName is null)
        {
            return Results.Unauthorized();
        }

        if (!Enum.TryParse<NoteEntityType>(entityType, out var parsedEntityType))
        {
            return Results.BadRequest("Invalid entity type.");
        }

        if (!Guid.TryParse(entityId, out var parsedEntityId))
        {
            return Results.BadRequest("Invalid entity ID.");
        }

        using var stream = file.OpenReadStream();

        var result = await fileService.UploadFileAsync(
            null,
            parsedEntityType,
            parsedEntityId,
            stream,
            file.FileName,
            file.Length,
            file.ContentType ?? "application/octet-stream",
            userName);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Error);
        }

        return Results.Ok(result.Value);
    }
}
