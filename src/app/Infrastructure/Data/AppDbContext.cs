using AbbaFleet.Features.Drivers;
using AbbaFleet.Features.Trucks;
using AbbaFleet.Shared;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AbbaFleet.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
{
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<Truck> Trucks { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<InvestmentEntry> InvestmentEntries { get; set; }
    public DbSet<AttachedFile> AttachedFiles { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
