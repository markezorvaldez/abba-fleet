using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AbbaFleet.Infrastructure.Data.EntityConfigurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        var comparer = new ValueComparer<HashSet<Permission>>(
            (a, b) => a != null && b != null && a.SetEquals(b),
            c => c.Aggregate(0, (acc, v) => HashCode.Combine(acc, v.GetHashCode())),
            c => c.ToHashSet());

        // The public Permissions property is IReadOnlySet<Permission>, which EF Core cannot use as a
        // converter model type. We ignore it and map the private backing field directly so that the
        // domain encapsulation (IReadOnlySet) and EF Core's concrete-type requirement both hold.
        builder.Ignore(u => u.Permissions);

        builder.Property<HashSet<Permission>>("_permissions")
            .HasField("_permissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("Permissions")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<HashSet<Permission>>(v, JsonOptions) ?? new HashSet<Permission>())
            .HasColumnType("text")
            .Metadata.SetValueComparer(comparer);
    }
}
