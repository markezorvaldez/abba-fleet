using AbbaFleet.Features.Clients;
using AbbaFleet.Features.Drivers;
using AbbaFleet.Features.Trucks;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Truck> Trucks { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<InvestmentEntry> InvestmentEntries { get; set; }
    public DbSet<AttachedFile> AttachedFiles { get; set; }
    public DbSet<DriverAuditEntry> DriverAuditEntries { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditableEntities = ChangeTracker.Entries<IAuditable>()
                                             .Select(e => e.Entity)
                                             .Where(e => e.AuditLog.Count > 0)
                                             .ToList();

        foreach (var entity in auditableEntities)
        {
            foreach (var entry in entity.AuditLog)
            {
                Add(entry);
            }

            entity.ClearAuditLog();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Ignore<AuditEntryBase>();
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
