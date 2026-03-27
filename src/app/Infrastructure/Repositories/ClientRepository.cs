using AbbaFleet.Features.Clients;
using AbbaFleet.Features.Drivers;
using AbbaFleet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Repositories;

public class ClientRepository(IDbContextFactory<AppDbContext> factory) : IClientRepository
{
    public async Task<IReadOnlyList<Client>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Clients.OrderBy(c => c.CompanyName).ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Clients.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Client?> GetByCompanyNameAsync(string companyName)
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Clients
                       .FirstOrDefaultAsync(c => c.CompanyName == companyName);
    }

    public async Task<IReadOnlyList<ClientAssignedTruckDto>> GetAssignedTrucksAsync(Guid clientId)
    {
        await using var db = await factory.CreateDbContextAsync();

        return await db.Trucks
                       .Where(t => t.ClientId == clientId)
                       .GroupJoin(
                           db.Set<Driver>(),
                           t => t.DriverId,
                           d => d.Id,
                           (t, drivers) => new
                           {
                               Truck = t,
                               Drivers = drivers
                           })
                       .SelectMany(
                           x => x.Drivers.DefaultIfEmpty(),
                           (x, d) => new ClientAssignedTruckDto(
                               x.Truck.Id,
                               x.Truck.PlateNumber,
                               x.Truck.TruckModel,
                               x.Truck.OwnershipType.ToString(),
                               d != null ? d.FullName : null,
                               x.Truck.IsActive))
                       .OrderBy(t => t.PlateNumber)
                       .ToListAsync();
    }

    public async Task AddAsync(Client client)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Clients.Add(client);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Client client)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Clients.Update(client);
        await db.SaveChangesAsync();
    }
}
