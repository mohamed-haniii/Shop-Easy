using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopEasy.Models;

namespace ShopEasy.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // Rule 1: Map to table Customers under schema shop
        builder.ToTable("Customers", "shop");

        // Rule 2: Set CustomerId as primary key; rename column to customer_id in the DB
        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerId)
               .HasColumnName("customer_id");

        // Rule 3: FullName — required, max 150 chars, column name full_name, with comment
        builder.Property(c => c.FullName)
               .IsRequired()
               .HasMaxLength(150)
               .HasColumnName("full_name")
               .HasComment("Customer full legal name");

        // Rule 4: Email — required, max 250 chars, unique index
        builder.Property(c => c.Email)
               .IsRequired()
               .HasMaxLength(250);

        builder.HasIndex(c => c.Email)
               .IsUnique()
               .HasDatabaseName("IX_Customers_Email");

        // Rule 5: PhoneNumber — optional, max 20 chars
        builder.Property(c => c.PhoneNumber)
               .HasMaxLength(20)
               .IsRequired(false);

        // Rule 6: CreatedAt — default value via SQL function GETUTCDATE()
        builder.Property(c => c.CreatedAt)
               .HasDefaultValueSql("GETUTCDATE()");
    }
}
