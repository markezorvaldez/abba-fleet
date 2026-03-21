using AbbaFleet.Features.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbbaFleet.Infrastructure.Data.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.PhoneNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.FacebookLink)
            .HasMaxLength(100);

        builder.Property(d => d.Address)
            .HasMaxLength(100);

        builder.Property(d => d.DateStarted)
            .IsRequired();

        builder.HasIndex(d => d.IsActive);
    }
}
