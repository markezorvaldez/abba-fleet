namespace AbbaFleet.Shared;

public sealed record FileDto(
    Guid Id,
    Guid? NoteId,
    string? NoteTitle,
    string FileName,
    long FileSize,
    string ContentType,
    string UploadedBy,
    DateTimeOffset UploadedAt);
