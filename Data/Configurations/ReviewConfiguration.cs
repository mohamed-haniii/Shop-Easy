using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        // Rule 1: Map to table Reviews under schema shop
        builder.ToTable("Reviews", "shop");

        // Rule 2: Rating required; Comment optional max 1000 chars
        builder.Property(r => r.Rating)
               .IsRequired();

        builder.Property(r => r.Comment)
               .HasMaxLength(1000)
               .IsRequired(false);

        // Rule 3: CreatedAt — default value GETUTCDATE()
        builder.Property(r => r.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        // Rule 4: Composite index on (ProductId, CustomerId)
        builder.HasIndex(r => new { r.ProductId, r.CustomerId })
               .HasDatabaseName("IX_Reviews_Product_Customer");

        // Rule 5: Relationship to Product — OnDelete Cascade
        // When a product is deleted, its reviews are deleted too
        builder.HasOne(r => r.Product)
               .WithMany(p => p.Reviews)
               .HasForeignKey(r => r.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        // Rule 6: Relationship to Customer — OnDelete Restrict
        // Restrict to avoid multiple cascade paths (SQL Server limitation)
        builder.HasOne(r => r.Customer)
               .WithMany(c => c.Reviews)
               .HasForeignKey(r => r.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
