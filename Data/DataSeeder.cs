using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShopEasy.Models;

namespace ShopEasy.Data;

// US-003: Seed initial data from JSON files
public static class DataSeeder
{
    // US-003: JSON files deserialized using System.Text.Json
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task SeedAsync(AppDbContext context)
    {
        // US-003: Seeding is skipped if data already exists (idempotent)
        if (await context.Customers.AnyAsync())
        {
            Console.WriteLine("⏭️  Data already seeded — skipping.");
            return;
        }

        // US-003: Data inserted using AddRange() inside a transaction
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // ── Dependency order: parents before children ──────────────────────────

            // 1. customers.json
            var customerDtos = LoadJson<CustomerSeedDto>("Models/JsonData/customers.json");
            var customers = customerDtos.Select(d => new Customer
            {
                FullName    = d.FullName,
                Email       = d.Email,
                PhoneNumber = d.PhoneNumber,
                CreatedAt   = d.CreatedAt
            }).ToList();
            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ customers.json     — {customers.Count} records");

            // 2. categories.json
            var categoryDtos = LoadJson<CategorySeedDto>("Models/JsonData/categories.json");
            var categories = categoryDtos.Select(d => new Category
            {
                Name        = d.Name,
                Slug        = d.Slug,
                Description = d.Description
            }).ToList();
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ categories.json    — {categories.Count} records");

            // 3. products.json — assign to first category (FK requirement)
            var productDtos = LoadJson<ProductSeedDto>("Models/JsonData/products.json");
            var firstCatId  = categories.First().CategoryId;
            var products = productDtos.Select(d => new Product
            {
                Name          = d.Name,
                SKU           = d.SKU,
                Price         = d.Price,
                StockQuantity = d.StockQuantity,
                IsActive      = d.IsActive,
                CategoryId    = firstCatId
            }).ToList();
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ products.json      — {products.Count} records");

            // 4. tags.json
            var tagDtos = LoadJson<TagSeedDto>("Models/JsonData/tags.json");
            var tags = tagDtos.Select(d => new Tag { Name = d.Name }).ToList();
            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ tags.json          — {tags.Count} records");

            // 5. orders.json — link to saved customer IDs
            var orderDtos = LoadJson<OrderSeedDto>("Models/JsonData/orders.json");
            // Distribute orders across available customers round-robin
            var orders = orderDtos.Select((d, i) => new Order
            {
                CustomerId  = customers[i % customers.Count].CustomerId,
                Status      = Enum.Parse<OrderStatus>(d.Status),
                TotalAmount = d.TotalAmount,
                PlacedAt    = d.PlacedAt,
                ShippedAt   = d.ShippedAt
            }).ToList();
            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ orders.json        — {orders.Count} records");

            // 6. orderItems.json — link to saved order/product IDs
            var itemDtos = LoadJson<OrderItemSeedDto>("Models/JsonData/orderItems.json");
            var orderItems = itemDtos.Select((d, i) => new OrderItem
            {
                OrderId   = orders[i % orders.Count].OrderId,
                ProductId = products[i % products.Count].ProductId,
                Quantity  = d.Quantity,
                UnitPrice = d.UnitPrice
            }).ToList();
            await context.OrderItems.AddRangeAsync(orderItems);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ orderItems.json    — {orderItems.Count} records");

            // 7. reviews.json — link to saved product/customer IDs
            var reviewDtos = LoadJson<ReviewSeedDto>("Models/JsonData/reviews.json");
            var reviews = reviewDtos.Select((d, i) => new Review
            {
                ProductId  = products[i % products.Count].ProductId,
                CustomerId = customers[i % customers.Count].CustomerId,
                Rating     = d.Rating,
                Comment    = d.Comment,
                CreatedAt  = d.CreatedAt
            }).ToList();
            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ reviews.json       — {reviews.Count} records");

            // 8. discounts.json
            var discountDtos = LoadJson<DiscountSeedDto>("Models/JsonData/discounts.json");
            var discounts = discountDtos.Select(d => new Discount
            {
                Code        = d.Code,
                Percentage  = d.Percentage,
                ExpiresAt   = d.ExpiresAt,
                IsActive    = d.IsActive,
                MaxUses     = d.MaxUses,
                CurrentUses = d.CurrentUses
            }).ToList();
            await context.Discounts.AddRangeAsync(discounts);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ discounts.json     — {discounts.Count} records");

            await transaction.CommitAsync();
            Console.WriteLine("\n✅ All 8 JSON files seeded successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"❌ Seeding failed and rolled back: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            throw;
        }
    }

    private static List<T> LoadJson<T>(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"⚠️  {path} not found.");
            return new List<T>();
        }
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
    }
}

// ── Seed DTOs (no explicit IDs — SQL Server generates them) ─────────────────
file record CustomerSeedDto(string FullName, string Email, string? PhoneNumber, DateTime CreatedAt);
file record CategorySeedDto(string Name, string Slug, string? Description);
file record ProductSeedDto(string Name, string SKU, decimal Price, int StockQuantity, bool IsActive);
file record TagSeedDto(string Name);
file record OrderSeedDto(string Status, decimal TotalAmount, DateTime PlacedAt, DateTime? ShippedAt);
file record OrderItemSeedDto(int Quantity, decimal UnitPrice);
file record ReviewSeedDto(int Rating, string? Comment, DateTime CreatedAt);
file record DiscountSeedDto(string Code, decimal Percentage, DateTime ExpiresAt, bool IsActive, int MaxUses, int CurrentUses);
