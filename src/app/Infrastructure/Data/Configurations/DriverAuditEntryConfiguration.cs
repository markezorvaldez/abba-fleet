using AbbaFleet.Features.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbbaFleet.Infrastructure.Data.Configurations;

public class DriverAuditEntryConfiguration : IEntityTypeConfiguration<DriverAuditEntry>
{
    public void Configure(EntityTypeBuilder<DriverAuditEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DriverId)
               .IsRequired();

        builder.Property(e => e.ActionType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(e => e.ChangedBy)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.Reason)
               .HasMaxLength(500);

        builder.HasOne<Driver>()
               .WithMany()
               .HasForeignKey(e => e.DriverId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.DriverId);

        builder.HasIndex(e => e.Timestamp);
    }
}
