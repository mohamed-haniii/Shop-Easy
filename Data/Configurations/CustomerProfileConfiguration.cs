using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        // Rule 1: Map to table CustomerProfiles under schema shop
        builder.ToTable("CustomerProfiles", "shop");

        // Rule 2: Address max 300, City max 100, PostalCode max 20
        builder.Property(cp => cp.Address)
               .HasMaxLength(300);

        builder.Property(cp => cp.City)
               .HasMaxLength(100);

        builder.Property(cp => cp.PostalCode)
               .HasMaxLength(20);

        // Rule 3: NationalId — max 30 chars, column type nchar(14)
        builder.Property(cp => cp.NationalId)
               .HasMaxLength(30)
               .HasColumnType("nchar(14)");

        // Rule 4 + Rule 5: One-to-one with Customer using HasForeignKey<CustomerProfile>
        // OnDelete Cascade — profile is removed when customer is deleted
        // Both rules are chained in a single fluent call to avoid EF conflict
        builder.HasOne(cp => cp.Customer)
               .WithOne(c => c.Profile)
               .HasForeignKey<CustomerProfile>(cp => cp.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
