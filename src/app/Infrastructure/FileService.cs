using System.Security.Claims;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace AbbaFleet.Infrastructure;

public class FileService(
    IFileRepository fileRepository,
    INoteRepository noteRepository,
    IFileStorageService storageService,
    AuthenticationStateProvider authStateProvider,
    ILogger<FileService> logger) : IFileService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<IReadOnlyList<FileDto>> GetFilesForEntityAsync(NoteEntityType entityType, Guid entityId)
    {
        var files = await fileRepository.GetByEntityAsync(entityType, entityId);
        var noteTitles = await BuildNoteTitleLookupAsync(files);
        return files.Select(f => MapToDto(f, noteTitles)).ToList();
    }

    public async Task<IReadOnlyList<FileDto>> GetFilesForNoteAsync(Guid noteId)
    {
        var files = await fileRepository.GetByNoteIdAsync(noteId);
        var note = files.Any() ? await noteRepository.GetByIdAsync(noteId) : null;
        var noteTitle = note?.Title;
        return files.Select(f => MapToDto(f, noteTitle)).ToList();
    }

    public async Task<Result<FileDto>> UploadFileAsync(
        Guid? noteId, NoteEntityType entityType, Guid entityId, IBrowserFile file)
    {
        if (file.Size > MaxFileSizeBytes)
        {
            return $"File exceeds the maximum allowed size of 10 MB.";
        }

        // Read the BrowserFileStream on the caller's context (Blazor render context)
        // before delegating to the stream-based overload, to avoid deadlocks.
        await using var browserStream = file.OpenReadStream(MaxFileSizeBytes);
        using var memoryStream = new MemoryStream();
        await browserStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return await UploadFileAsync(noteId, entityType, entityId, memoryStream, file.Name, file.Size, file.ContentType);
    }

    public async Task<Result<FileDto>> UploadFileAsync(
        Guid? noteId, NoteEntityType entityType, Guid entityId,
        Stream stream, string fileName, long fileSize, string contentType)
    {
        if (fileSize > MaxFileSizeBytes)
        {
            return $"File exceeds the maximum allowed size of 10 MB.";
        }

        if (noteId.HasValue)
        {
            var note = await noteRepository.GetByIdAsync(noteId.Value);

            if (note is null)
            {
                return "The specified note does not exist.";
            }
        }

        var userName = await GetCurrentUserNameAsync();

        if (userName is null)
        {
            return "Unable to determine the current user.";
        }

        var storagePath = await storageService.SaveAsync(stream, fileName, entityType, entityId);

        var attachedFile = new AttachedFile(
            noteId,
            entityType,
            entityId,
            fileName,
            fileSize,
            contentType,
            storagePath,
            userName);

        await fileRepository.AddAsync(attachedFile);

        logger.LogInformation(
            "File {FileName} uploaded for {EntityType} {EntityId} by {UserName}",
            fileName, entityType, entityId, userName);

        string? noteTitle = null;

        if (noteId.HasValue)
        {
            var note = await noteRepository.GetByIdAsync(noteId.Value);
            noteTitle = note?.Title;
        }

        return MapToDto(attachedFile, noteTitle);
    }

    public async Task<Result<bool>> DeleteFileAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);

        if (file is null)
        {
            return "File not found.";
        }

        var userName = await GetCurrentUserNameAsync();

        if (userName is null)
        {
            return "Unable to determine the current user.";
        }

        logger.LogInformation(
            "File {FileId} ({FileName}) deleted by {UserName}",
            file.Id, file.FileName, userName);

        await fileRepository.DeleteAsync(file);
        await storageService.DeleteAsync(file.StoragePath);

        return true;
    }

    public async Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);

        if (file is null)
        {
            return null;
        }

        var stream = storageService.OpenRead(file.StoragePath);
        return (stream, file.ContentType, file.FileName);
    }

    private async Task<string?> GetCurrentUserNameAsync()
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirstValue(ClaimTypes.Name);
    }

    private async Task<Dictionary<Guid, string>> BuildNoteTitleLookupAsync(IReadOnlyList<AttachedFile> files)
    {
        var noteIds = files
            .Where(f => f.NoteId.HasValue)
            .Select(f => f.NoteId!.Value)
            .Distinct()
            .ToList();

        var lookup = new Dictionary<Guid, string>();

        foreach (var noteId in noteIds)
        {
            var note = await noteRepository.GetByIdAsync(noteId);

            if (note is not null)
            {
                lookup[noteId] = note.Title;
            }
        }

        return lookup;
    }

    private static FileDto MapToDto(AttachedFile f, Dictionary<Guid, string> noteTitles)
    {
        string? noteTitle = f.NoteId.HasValue && noteTitles.TryGetValue(f.NoteId.Value, out var title)
            ? title
            : null;

        return MapToDto(f, noteTitle);
    }

    private static FileDto MapToDto(AttachedFile f, string? noteTitle) => new(
        f.Id,
        f.NoteId,
        noteTitle,
        f.FileName,
        f.FileSize,
        f.ContentType,
        f.UploadedBy,
        f.UploadedAt);
}
