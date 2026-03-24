using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        // Rule 1: Map to table Tags under schema shop
        builder.ToTable("Tags", "shop");

        // Rule 2: Name — required, max 50 chars
        builder.Property(t => t.Name)
               .IsRequired()
               .HasMaxLength(50);

        // Rule 3: Unique index on Name
        builder.HasIndex(t => t.Name)
               .IsUnique()
               .HasDatabaseName("IX_Tags_Name");
    }
}
