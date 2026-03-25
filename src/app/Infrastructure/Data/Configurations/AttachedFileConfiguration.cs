using AbbaFleet.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbbaFleet.Infrastructure.Data.Configurations;

public class AttachedFileConfiguration : IEntityTypeConfiguration<AttachedFile>
{
    public void Configure(EntityTypeBuilder<AttachedFile> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.EntityType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(f => f.EntityId)
               .IsRequired();

        builder.Property(f => f.FileName)
               .IsRequired()
               .HasMaxLength(260);

        builder.Property(f => f.ContentType)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(f => f.StoragePath)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(f => f.UploadedBy)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasOne<Note>()
               .WithMany()
               .HasForeignKey(f => f.NoteId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        builder.HasIndex(f => new
        {
            f.EntityType,
            f.EntityId
        });

        builder.HasIndex(f => f.NoteId);
    }
}
