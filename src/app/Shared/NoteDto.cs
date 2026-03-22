namespace AbbaFleet.Shared;

public sealed record NoteDto(
    Guid Id,
    string Title,
    string Body,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    string? ModifiedBy,
    DateTimeOffset? ModifiedAt);
