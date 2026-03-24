using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        // Rule 1: Map to table ProductTags under schema shop
        builder.ToTable("ProductTags", "shop");

        // Rule 2: Composite primary key
        builder.HasKey(pt => new { pt.ProductId, pt.TagId });

        // Rule 3: Relationship to Product
        builder.HasOne(pt => pt.Product)
               .WithMany(p => p.ProductTags)
               .HasForeignKey(pt => pt.ProductId);

        // Rule 4: Relationship to Tag
        builder.HasOne(pt => pt.Tag)
               .WithMany(t => t.ProductTags)
               .HasForeignKey(pt => pt.TagId);
    }
}
