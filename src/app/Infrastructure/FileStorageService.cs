using AbbaFleet.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AbbaFleet.Infrastructure;

public class FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger) : IFileStorageService
{
    private string BasePath => configuration["FileStorage:BasePath"] ?? "uploads";

    public async Task<string> SaveAsync(Stream stream, string fileName, NoteEntityType entityType, Guid entityId)
    {
        var sanitized = SanitizeFileName(fileName);
        var folder = Path.Combine(BasePath, entityType.ToString().ToLowerInvariant(), entityId.ToString());

        Directory.CreateDirectory(folder);

        var uniqueName = $"{Guid.NewGuid()}_{sanitized}";
        var fullPath = Path.Combine(folder, uniqueName);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream);

        var storagePath = Path.Combine(entityType.ToString().ToLowerInvariant(), entityId.ToString(), uniqueName)
            .Replace('\\', '/');

        logger.LogInformation("File saved to {StoragePath}", storagePath);

        return storagePath;
    }

    public Task DeleteAsync(string storagePath)
    {
        var fullPath = Path.Combine(BasePath, storagePath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            logger.LogInformation("File deleted: {StoragePath}", storagePath);
        }
        else
        {
            logger.LogWarning("File not found for deletion: {StoragePath}", storagePath);
        }

        return Task.CompletedTask;
    }

    public Stream OpenRead(string storagePath)
    {
        var fullPath = Path.Combine(BasePath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));
        return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
    }
}
