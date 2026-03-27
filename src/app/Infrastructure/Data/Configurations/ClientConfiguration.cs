using AbbaFleet.Features.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbbaFleet.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompanyName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Address).HasMaxLength(200);
        builder.Property(c => c.TaxRate).HasPrecision(5, 2);
        builder.Property(c => c.IsActive).IsRequired();

        builder.HasIndex(c => c.CompanyName).IsUnique();
        builder.HasIndex(c => c.IsActive);
    }
}
