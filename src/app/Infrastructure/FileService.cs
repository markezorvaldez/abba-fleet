using AbbaFleet.Shared;

namespace AbbaFleet.Infrastructure;

public class FileService(
    IFileRepository repository,
    IFileStorageService storageService,
    ILogger<FileService> logger) : IFileService
{
    public async Task<IReadOnlyList<FileDto>> GetFilesForEntityAsync(NoteEntityType entityType, Guid entityId)
    {
        var files = await repository.GetByEntityAsync(entityType, entityId);

        return files.Select(MapToDto).ToList();
    }

    public async Task<Result<FileDto>> UploadFileAsync(
        Guid? noteId,
        NoteEntityType entityType,
        Guid entityId,
        Stream stream,
        string fileName,
        long fileSize,
        string contentType,
        string uploadedBy)
    {
        var storagePath = await storageService.SaveAsync(stream, fileName, entityType, entityId);
        var file = new AttachedFile(noteId, entityType, entityId, fileName, fileSize, contentType, storagePath, uploadedBy);
        await repository.AddAsync(file);

        logger.LogInformation(
            "File {FileId} uploaded for {EntityType} {EntityId} by {UploadedBy}",
            file.Id,
            entityType,
            entityId,
            uploadedBy);

        return MapToDto(file);
    }

    public async Task<Result<bool>> DeleteFileAsync(Guid fileId)
    {
        var file = await repository.GetByIdAsync(fileId);

        if (file is null)
        {
            return "File not found.";
        }

        await storageService.DeleteAsync(file.StoragePath);
        await repository.DeleteAsync(file);

        logger.LogInformation("File {FileId} deleted.", fileId);

        return true;
    }

    public async Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(Guid fileId)
    {
        var file = await repository.GetByIdAsync(fileId);

        if (file is null)
        {
            return null;
        }

        var stream = await storageService.OpenReadAsync(file.StoragePath);

        if (stream is not null)
        {
            return (stream, file.ContentType, file.FileName);
        }

        // R2 orphan: object was deleted directly in Cloudflare dashboard
        logger.LogWarning(
            "File {FileId} (StoragePath={StoragePath}) not found in R2 — removing DB record.",
            file.Id,
            file.StoragePath);

        await repository.DeleteAsync(file);

        return null;
    }

    private static FileDto MapToDto(AttachedFile f)
    {
        return new FileDto(
            f.Id,
            f.NoteId,
            null,
            f.FileName,
            f.FileSize,
            f.ContentType,
            f.UploadedBy,
            f.UploadedAt);
    }
}
