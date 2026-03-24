using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        // Rule 1: Map to table Discounts under schema shop
        builder.ToTable("Discounts", "shop");

        // Rule 2: HasSequence<int>() is an extension on ModelBuilder — NOT on EntityTypeBuilder.
        // ⚠️  Calling builder.HasSequence() causes CS1929 compile error.
        // The sequence is defined in AppDbContext.OnModelCreating() instead:
        //   modelBuilder.HasSequence<int>("DiscountSeq", schema: "shop")
        //               .StartsAt(1000)
        //               .IncrementsBy(1);

        // Rule 3: DiscountId default value = NEXT VALUE FOR shop.DiscountSeq
        // The sequence is already created in AppDbContext — we just reference it here.
        builder.Property(d => d.DiscountId)
               .HasDefaultValueSql("NEXT VALUE FOR shop.DiscountSeq");

        // Rule 4: Code — required, max 30 chars, unique index
        builder.Property(d => d.Code)
               .IsRequired()
               .HasMaxLength(30);

        builder.HasIndex(d => d.Code)
               .IsUnique()
               .HasDatabaseName("IX_Discounts_Code");

        // Rule 5: Percentage — column type decimal(5,2)
        builder.Property(d => d.Percentage)
               .HasColumnType("decimal(5,2)");

        // Rule 6: IsActive default true; MaxUses default 100
        builder.Property(d => d.IsActive)
               .HasDefaultValue(true);

        builder.Property(d => d.MaxUses)
               .HasDefaultValue(100);
    }
}
