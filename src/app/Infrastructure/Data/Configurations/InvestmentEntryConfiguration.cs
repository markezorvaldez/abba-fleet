using AbbaFleet.Features.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbbaFleet.Infrastructure.Data.Configurations;

public class InvestmentEntryConfiguration : IEntityTypeConfiguration<InvestmentEntry>
{
    public void Configure(EntityTypeBuilder<InvestmentEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TruckId)
               .IsRequired();

        builder.HasIndex(e => e.TruckId);

        builder.Property(e => e.Type)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(e => e.Amount)
               .IsRequired()
               .HasPrecision(18, 2);

        builder.Property(e => e.Date)
               .IsRequired();

        builder.Property(e => e.Description)
               .HasMaxLength(500);

        builder.Property(e => e.CreatedBy)
               .IsRequired()
               .HasMaxLength(100);
    }
}
