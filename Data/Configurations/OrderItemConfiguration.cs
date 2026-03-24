using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        // Rule 1: Map to table OrderItems under schema shop
        builder.ToTable("OrderItems", "shop");

        // Rule 2: UnitPrice decimal(18,2); Quantity required
        builder.Property(oi => oi.UnitPrice)
               .HasColumnType("decimal(18,2)");

        builder.Property(oi => oi.Quantity)
               .IsRequired();

        // Rule 3: Composite index on (OrderId, ProductId)
        builder.HasIndex(oi => new { oi.OrderId, oi.ProductId })
               .HasDatabaseName("IX_OrderItems_Order_Product");

        // Rule 4: Relationship to Order — OnDelete Cascade
        builder.HasOne(oi => oi.Order)
               .WithMany(o => o.OrderItems)
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // Rule 5: Relationship to Product — OnDelete Restrict
        builder.HasOne(oi => oi.Product)
               .WithMany(p => p.OrderItems)
               .HasForeignKey(oi => oi.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
