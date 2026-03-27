using AbbaFleet.Features.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbbaFleet.Infrastructure.Data.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Role).HasMaxLength(100);
        builder.Property(c => c.PhoneNumber).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(100);

        builder.HasOne<Client>()
               .WithMany()
               .HasForeignKey(c => c.ClientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ClientId);
    }
}
