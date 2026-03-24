using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShopEasy.Models;

namespace ShopEasy.Data;

// US-001: AppDbContext inherits from DbContext
public class AppDbContext : DbContext
{
    // US-001: All 12 entities have corresponding DbSet<T> properties
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Discount> Discounts => Set<Discount>();

    // US-001: Connection string is read from configuration, not hardcoded
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder
            // US-050: UseLazyLoadingProxies — enables lazy loading via virtual nav properties
            .UseLazyLoadingProxies()
            .UseSqlServer(connectionString);
    }

    // US-001: OnModelCreating calls ApplyConfigurationsFromAssembly
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DiscountConfiguration Rule 2:
        // HasSequence<T> MUST be called on ModelBuilder (not EntityTypeBuilder).
        // Defined here so DiscountConfiguration can safely reference it via HasDefaultValueSql.
        modelBuilder.HasSequence<int>("DiscountSeq", schema: "shop")
                    .StartsAt(1000)
                    .IncrementsBy(1);

        // Automatically applies all IEntityTypeConfiguration<T> classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
