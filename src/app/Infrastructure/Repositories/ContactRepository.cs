using AbbaFleet.Features.Clients;
using AbbaFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Repositories;

public class ContactRepository(IDbContextFactory<AppDbContext> factory) : IContactRepository
{
    public async Task<IReadOnlyList<Contact>> GetByClientIdAsync(Guid clientId)
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Contacts
                       .Where(c => c.ClientId == clientId)
                       .OrderBy(c => c.FullName)
                       .ToListAsync();
    }

    public async Task<Contact?> GetByIdAsync(Guid id)
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Contacts.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Contact contact)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Contacts.Add(contact);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Contact contact)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Contacts.Update(contact);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Contact contact)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Contacts.Remove(contact);
        await db.SaveChangesAsync();
    }
}
