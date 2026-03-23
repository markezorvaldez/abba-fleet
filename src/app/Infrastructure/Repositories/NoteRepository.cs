using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Shared;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Repositories;

public class NoteRepository(IDbContextFactory<AppDbContext> dbFactory) : INoteRepository
{
    public async Task<IReadOnlyList<Note>> GetByEntityAsync(NoteEntityType entityType, Guid entityId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Notes
            .Where(n => n.EntityType == entityType && n.EntityId == entityId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Notes.FindAsync(id);
    }

    public async Task AddAsync(Note note)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Notes.Add(note);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Note note)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Notes.Update(note);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Note note)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Notes.Remove(note);
        await db.SaveChangesAsync();
    }
}
