using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        // Rule 1: Map to table ProductImages under schema shop
        builder.ToTable("ProductImages", "shop");

        // Rule 2: Url required max 500, AltText optional max 200
        builder.Property(pi => pi.Url)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(pi => pi.AltText)
               .HasMaxLength(200)
               .IsRequired(false);

        // Rule 3: IsPrimary — default value false
        builder.Property(pi => pi.IsPrimary)
               .HasDefaultValue(false);

        // Rule 4: One-to-one with Product, cascade delete
        builder.HasOne(pi => pi.Product)
               .WithOne(p => p.ProductImage)
               .HasForeignKey<ProductImage>(pi => pi.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
