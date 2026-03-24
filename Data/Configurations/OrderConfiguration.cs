using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Rule 1: Map to table Orders under schema shop
        builder.ToTable("Orders", "shop");

        // Rule 2: Status — store as string, max 30 chars, default Pending
        builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasMaxLength(30)
               .HasDefaultValue(OrderStatus.Pending);

        // Rule 3: TotalAmount — column type decimal(18,2)
        builder.Property(o => o.TotalAmount)
               .HasColumnType("decimal(18,2)");

        // Rule 4: PlacedAt — default value GETUTCDATE()
        builder.Property(o => o.PlacedAt)
               .HasDefaultValueSql("GETUTCDATE()");

        // Rule 5: Filtered index on Status = 'Pending'
        builder.HasIndex(o => o.Status)
               .HasFilter("[Status] = 'Pending'")
               .HasDatabaseName("IX_Orders_PendingStatus");

        // Rule 6: One-to-many with Customer, OnDelete Restrict
        builder.HasOne(o => o.Customer)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
