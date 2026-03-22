using AbbaFleet.Features.Drivers;
using AbbaFleet.Features.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbbaFleet.Infrastructure.Data.Configurations;

public class TruckConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.PlateNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.TruckModel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.OwnershipType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.DateAcquired)
            .IsRequired();

        builder.HasOne<Driver>()
            .WithMany()
            .HasForeignKey(t => t.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.PlateNumber).IsUnique();
        builder.HasIndex(t => t.IsActive);
    }
}
