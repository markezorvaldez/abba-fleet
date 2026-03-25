using AbbaFleet.Features.Trucks;
using AbbaFleet.Infrastructure.Data;
using AbbaFleet.Shared;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Repositories;

public class TruckRepository(IDbContextFactory<AppDbContext> dbFactory) : ITruckRepository
{
    public async Task<IReadOnlyList<(Truck Truck, string? DriverName)>> GetAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var results = await db.Trucks
                              .Join(
                                  db.Drivers,
                                  t => t.DriverId,
                                  d => d.Id,
                                  (t, d) => new
                                  {
                                      Truck = t,
                                      DriverName = (string?)d.FullName
                                  })
                              .Union(
                                  db.Trucks
                                    .Where(t => t.DriverId == null)
                                    .Select(t => new
                                    {
                                        Truck = t,
                                        DriverName = (string?)null
                                    }))
                              .OrderBy(x => x.Truck.PlateNumber)
                              .ToListAsync();

        return results.Select(r => (r.Truck, r.DriverName)).ToList();
    }

    public async Task<(Truck Truck, string? DriverName)?> GetByIdAsync(Guid id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var truck = await db.Trucks.FindAsync(id);

        if (truck is null)
        {
            return null;
        }

        string? driverName = null;

        if (truck.DriverId.HasValue)
        {
            driverName = await db.Drivers
                                 .Where(d => d.Id == truck.DriverId.Value)
                                 .Select(d => d.FullName)
                                 .FirstOrDefaultAsync();
        }

        return (truck, driverName);
    }

    public async Task AddAsync(Truck truck)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Trucks.Add(truck);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Truck truck)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Trucks.Update(truck);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Truck truck)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Trucks.Remove(truck);
        await db.SaveChangesAsync();
    }

    public async Task<bool> ExistsWithPlateNumberAsync(string plateNumber, Guid? excludeId = null)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var query = db.Trucks.Where(t => t.PlateNumber == plateNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IReadOnlyList<LookupItem>> GetActiveDriverOptionsAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Drivers
                       .Where(d => d.IsActive)
                       .OrderBy(d => d.FullName)
                       .Select(d => new LookupItem(d.Id, d.FullName))
                       .ToListAsync();
    }

    public async Task<(Truck Truck, string? DriverName)?> GetByDriverIdAsync(Guid driverId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var truck = await db.Trucks
                            .Where(t => t.DriverId == driverId)
                            .FirstOrDefaultAsync();

        if (truck is null)
        {
            return null;
        }

        var driverName = await db.Drivers
                                 .Where(d => d.Id == driverId)
                                 .Select(d => d.FullName)
                                 .FirstOrDefaultAsync();

        return (truck, driverName);
    }

    public async Task<DriverLookup?> GetDriverLookupAsync(Guid driverId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Drivers
                       .Where(d => d.Id == driverId)
                       .Select(d => new DriverLookup(d.Id, d.FullName, d.IsActive))
                       .FirstOrDefaultAsync();
    }
}
