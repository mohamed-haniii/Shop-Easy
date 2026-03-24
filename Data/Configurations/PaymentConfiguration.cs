using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // Rule 1: Map to table Payments under schema shop
        builder.ToTable("Payments", "shop");

        // Rule 2: Method — required, max 50 chars (stored as string via enum conversion)
        builder.Property(p => p.Method)
               .HasConversion<string>()
               .IsRequired()
               .HasMaxLength(50);

        // Rule 3: Status — stored as string, max 30 chars
        builder.Property(p => p.Status)
               .HasConversion<string>()
               .HasMaxLength(30);

        // Rule 4: Amount — column type decimal(18,2)
        builder.Property(p => p.Amount)
               .HasColumnType("decimal(18,2)");

        // Rule 5: One-to-one with Order using HasForeignKey<Payment>, OnDelete Cascade
        builder.HasOne(p => p.Order)
               .WithOne(o => o.Payment)
               .HasForeignKey<Payment>(p => p.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
