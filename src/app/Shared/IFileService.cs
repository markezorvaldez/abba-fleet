namespace AbbaFleet.Shared;

public interface IFileService
{
    Task<IReadOnlyList<FileDto>> GetFilesForEntityAsync(NoteEntityType entityType, Guid entityId);
    Task<IReadOnlyList<FileDto>> GetFilesForNoteAsync(Guid noteId);
    Task<Result<FileDto>> UploadFileAsync(Guid? noteId, NoteEntityType entityType, Guid entityId, Stream stream, string fileName, long fileSize, string contentType);
    Task<Result<bool>> DeleteFileAsync(Guid fileId);
    Task<(Stream stream, string contentType, string fileName)?> DownloadFileAsync(Guid fileId);
}
