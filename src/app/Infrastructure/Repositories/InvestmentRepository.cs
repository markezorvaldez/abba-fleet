using AbbaFleet.Features.Trucks;
using AbbaFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Repositories;

public class InvestmentRepository(IDbContextFactory<AppDbContext> dbFactory) : IInvestmentRepository
{
    public async Task<IReadOnlyList<InvestmentEntry>> GetByTruckIdAsync(Guid truckId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.InvestmentEntries
                       .Where(e => e.TruckId == truckId)
                       .OrderBy(e => e.Date)
                       .ToListAsync();
    }

    public async Task AddAsync(InvestmentEntry entry)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.InvestmentEntries.Add(entry);
        await db.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalByTruckIdAsync(Guid truckId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.InvestmentEntries
                       .Where(e => e.TruckId == truckId)
                       .SumAsync(e => (decimal?)e.Amount)
            ?? 0m;
    }
}
