using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Shared;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Repositories;

public class FileRepository(IDbContextFactory<AppDbContext> dbFactory) : IFileRepository
{
    public async Task<IReadOnlyList<AttachedFile>> GetByEntityAsync(NoteEntityType entityType, Guid entityId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.AttachedFiles
                       .Where(f => f.EntityType == entityType && f.EntityId == entityId)
                       .OrderByDescending(f => f.UploadedAt)
                       .ToListAsync();
    }

    public async Task<IReadOnlyList<AttachedFile>> GetByNoteIdAsync(Guid noteId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.AttachedFiles
                       .Where(f => f.NoteId == noteId)
                       .OrderByDescending(f => f.UploadedAt)
                       .ToListAsync();
    }

    public async Task<AttachedFile?> GetByIdAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.AttachedFiles.FindAsync(id);
    }

    public async Task AddAsync(AttachedFile file)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.AttachedFiles.Add(file);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(AttachedFile file)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.AttachedFiles.Remove(file);
        await db.SaveChangesAsync();
    }
}
