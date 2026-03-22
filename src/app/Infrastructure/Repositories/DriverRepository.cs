using AbbaFleet.Features.Drivers;
using AbbaFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Repositories;

public class DriverRepository(IDbContextFactory<AppDbContext> dbFactory) : IDriverRepository
{
    public async Task<IReadOnlyList<Driver>> GetAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Drivers
            .OrderBy(d => d.FullName)
            .ToListAsync();
    }

    public async Task<Driver?> GetByIdAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Drivers.FindAsync(id);
    }

    public async Task AddAsync(Driver driver)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Drivers.Add(driver);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Driver driver)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Drivers.Update(driver);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Driver driver)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Drivers.Remove(driver);
        await db.SaveChangesAsync();
    }
}
