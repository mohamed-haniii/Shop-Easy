using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Rule 1: Map to table Categories under schema shop
        builder.ToTable("Categories", "shop");

        // Rule 2: Name required max 100, Slug required max 120
        builder.Property(c => c.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.Slug)
               .IsRequired()
               .HasMaxLength(120);

        // Rule 3: Unique index on Slug
        builder.HasIndex(c => c.Slug)
               .IsUnique()
               .HasDatabaseName("IX_Categories_Slug");

        // Rule 4: Exclude InternalNotes from the model
        builder.Ignore(c => c.InternalNotes);

        // Rule 5: Self-referencing one-to-many — ParentCategory / SubCategories
        // OnDelete Restrict to avoid accidental cascade deletion of subcategories
        builder.HasOne(c => c.ParentCategory)
               .WithMany(c => c.SubCategories)
               .HasForeignKey(c => c.ParentCategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
