using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Rule 1: Map to table Products under schema shop
        builder.ToTable("Products", "shop");

        // Rule 2: Price — column type decimal(18,2)
        builder.Property(p => p.Price)
               .HasColumnType("decimal(18,2)");

        // Rule 3: IsActive — default value true
        builder.Property(p => p.IsActive)
               .HasDefaultValue(true);

        // Rule 4: DisplayName — computed column stored in DB
        // Formula: [Name] + ' (' + [SKU] + ')'
        builder.Property(p => p.DisplayName)
               .HasComputedColumnSql("[Name] + ' (' + [SKU] + ')'", stored: true);

        // Rule 5: Global query filter — inactive products never returned automatically
        builder.HasQueryFilter(p => p.IsActive);

        // Rule 6: SKU unique index + one-to-many with Category using OnDelete Restrict
        builder.HasIndex(p => p.SKU)
               .IsUnique()
               .HasDatabaseName("IX_Products_SKU");

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
