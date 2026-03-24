using Microsoft.EntityFrameworkCore;
using ShopEasy.Data;
using ShopEasy.Models;

// ============================================================
//  ShopEasy — EF Core Console Application
//  ZagEng Backend .NET Track | 2024-2025
// ============================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var context = new AppDbContext();

// Ensure DB is created and migrations applied
await context.Database.EnsureCreatedAsync();

// US-003: Seed initial data
await DataSeeder.SeedAsync(context);

bool running = true;
while (running)
{
    Console.Clear();
    Console.WriteLine("╔══════════════════════════════════════╗");
    Console.WriteLine("║        ShopEasy — Main Menu          ║");
    Console.WriteLine("╠══════════════════════════════════════╣");
    Console.WriteLine("║  1.  Foundations & Setup             ║");
    Console.WriteLine("║  2.  Customer Management             ║");
    Console.WriteLine("║  3.  Product Catalog                 ║");
    Console.WriteLine("║  4.  Order Processing                ║");
    Console.WriteLine("║  5.  Payments & Discounts            ║");
    Console.WriteLine("║  6.  Advanced Queries & Loading      ║");
    Console.WriteLine("║  0.  Exit                            ║");
    Console.WriteLine("╚══════════════════════════════════════╝");
    Console.Write("Choose: ");

    var choice = Console.ReadLine();
    switch (choice)
    {
        case "1": await FoundationsMenu(); break;
        case "2": await CustomerMenu(); break;
        case "3": await ProductMenu(); break;
        case "4": await OrderMenu(); break;
        case "5": await PaymentsMenu(); break;
        case "6": await AdvancedMenu(); break;
        case "0": running = false; break;
        default:
            Console.WriteLine("Invalid choice.");
            Pause();
            break;
    }
}

Console.WriteLine("Goodbye!");

// ============================================================
//  SECTION 1 — Foundations & Setup
// ============================================================
async Task FoundationsMenu()
{
    Console.Clear();
    Console.WriteLine("=== Foundations & Setup ===");
    Console.WriteLine("1. US-001 — Show DB connection info");
    Console.WriteLine("2. US-002 — Show migration instructions");
    Console.WriteLine("3. US-003 — Re-run seeder");
    Console.Write("Choose: ");

    switch (Console.ReadLine())
    {
        case "1": await US001_ShowConnectionInfo(); break;
        case "2": US002_ShowMigrationInstructions(); break;
        case "3": await US003_ReseedData(); break;
    }
    Pause();
}

// US-001: AppDbContext with DbSets, connection from config, ApplyConfigurationsFromAssembly
async Task US001_ShowConnectionInfo()
{
    Console.WriteLine("\n--- US-001: Database Connection Info ---");

    // Verify we can connect
    bool canConnect = await context.Database.CanConnectAsync();
    Console.WriteLine($"Can connect to DB: {canConnect}");
    Console.WriteLine($"Provider:          {context.Database.ProviderName}");
    Console.WriteLine($"Database name:     {context.Database.GetDbConnection().Database}");
    Console.WriteLine($"Total DbSets:      12 (all entities registered)");
    Console.WriteLine("All 12 DbSets: Customers, CustomerProfiles, Categories, Products,");
    Console.WriteLine("               Tags, ProductTags, ProductImages, Orders,");
    Console.WriteLine("               OrderItems, Payments, Reviews, Discounts");
}

// US-002: Migration instructions — cannot run Add-Migration in code; show the commands
void US002_ShowMigrationInstructions()
{
    Console.WriteLine("\n--- US-002: Migration Commands ---");
    Console.WriteLine("Run these commands in the Package Manager Console or terminal:");
    Console.WriteLine();
    Console.WriteLine("  # Create initial migration:");
    Console.WriteLine("  dotnet ef migrations add InitialCreate");
    Console.WriteLine();
    Console.WriteLine("  # Apply migration to DB:");
    Console.WriteLine("  dotnet ef database update");
    Console.WriteLine();
    Console.WriteLine("  # Rollback to previous migration:");
    Console.WriteLine("  dotnet ef database update [PreviousMigrationName]");
    Console.WriteLine();
    Console.WriteLine("  # Generate SQL script:");
    Console.WriteLine("  dotnet ef migrations script");
    Console.WriteLine();
    Console.WriteLine("All tables will be created under schema 'shop'.");
}

// US-003: Seed data from JSON files (idempotent)
async Task US003_ReseedData()
{
    Console.WriteLine("\n--- US-003: Seeding Data from JSON Files ---");
    await DataSeeder.SeedAsync(context);
}

// ============================================================
//  SECTION 2 — Customer Management
// ============================================================
async Task CustomerMenu()
{
    Console.Clear();
    Console.WriteLine("=== Customer Management ===");
    Console.WriteLine("1. US-010 — Register new customer");
    Console.WriteLine("2. US-011 — View customer profile");
    Console.WriteLine("3. US-012 — Update customer address");
    Console.Write("Choose: ");

    switch (Console.ReadLine())
    {
        case "1": await US010_RegisterCustomer(); break;
        case "2": await US011_ViewCustomerProfile(); break;
        case "3": await US012_UpdateCustomerAddress(); break;
    }
    Pause();
}

// US-010: Register new customer
// Acceptance: Add(), email unique index, CreatedAt via HasDefaultValueSql,
//             CustomerProfile created in same transaction (1-to-1)
async Task US010_RegisterCustomer()
{
    Console.WriteLine("\n--- US-010: Register New Customer ---");

    Console.Write("Full Name:    ");
    var fullName = Console.ReadLine() ?? "Test User";

    Console.Write("Email:        ");
    var email = Console.ReadLine() ?? $"test{Guid.NewGuid():N}@example.com";

    Console.Write("Phone:        ");
    var phone = Console.ReadLine();

    Console.Write("Address:      ");
    var address = Console.ReadLine();

    Console.Write("City:         ");
    var city = Console.ReadLine();

    // US-010: Customer + CustomerProfile created in same transaction (1-to-1)
    await using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        var customer = new Customer
        {
            FullName = fullName,
            Email    = email,
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone
            // CreatedAt is set automatically by HasDefaultValueSql("GETUTCDATE()")
        };

        // US-010: Added using context.Customers.Add()
        context.Customers.Add(customer);
        await context.SaveChangesAsync(); // generates CustomerId

        var profile = new CustomerProfile
        {
            CustomerId  = customer.CustomerId,
            Address     = string.IsNullOrWhiteSpace(address) ? null : address,
            City        = string.IsNullOrWhiteSpace(city) ? null : city
        };

        context.CustomerProfiles.Add(profile);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        Console.WriteLine($"\n✅ Customer registered! ID = {customer.CustomerId}");
        Console.WriteLine($"   Email uniqueness enforced at DB level via IX_Customers_Email");
    }
    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Customers_Email") == true)
    {
        await transaction.RollbackAsync();
        Console.WriteLine("❌ A customer with this email already exists.");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

// US-011: View customer profile with eager loading
// Acceptance: .Include(Profile).Include(Orders), SingleOrDefault by ID
async Task US011_ViewCustomerProfile()
{
    Console.WriteLine("\n--- US-011: View Customer Profile ---");
    Console.Write("Customer ID: ");

    if (!int.TryParse(Console.ReadLine(), out int customerId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    // US-011: Eager loading — Profile and Orders loaded together
    var customer = await context.Customers
        .Include(c => c.Profile)
        .Include(c => c.Orders)
        .SingleOrDefaultAsync(c => c.CustomerId == customerId);

    // US-011: If not found, display meaningful message
    if (customer is null)
    {
        Console.WriteLine($"❌ No customer found with ID {customerId}.");
        return;
    }

    Console.WriteLine($"\n👤 Customer: {customer.FullName}");
    Console.WriteLine($"   Email:    {customer.Email}");
    Console.WriteLine($"   Phone:    {customer.PhoneNumber ?? "N/A"}");
    Console.WriteLine($"   Joined:   {customer.CreatedAt:yyyy-MM-dd}");

    if (customer.Profile is not null)
    {
        Console.WriteLine($"\n📍 Address: {customer.Profile.Address}, {customer.Profile.City}");
        Console.WriteLine($"   Postal:  {customer.Profile.PostalCode}");
    }
    else
    {
        Console.WriteLine("\n📍 No address on file.");
    }

    Console.WriteLine($"\n🛒 Orders ({customer.Orders.Count}):");
    foreach (var order in customer.Orders)
        Console.WriteLine($"   #{order.OrderId} | {order.Status} | {order.TotalAmount:C} | {order.PlacedAt:yyyy-MM-dd}");
}

// US-012: Update customer address
// Acceptance: explicit loading for CustomerProfile, SaveChanges, upsert pattern
async Task US012_UpdateCustomerAddress()
{
    Console.WriteLine("\n--- US-012: Update Customer Address ---");
    Console.Write("Customer ID: ");

    if (!int.TryParse(Console.ReadLine(), out int customerId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var customer = await context.Customers
        .SingleOrDefaultAsync(c => c.CustomerId == customerId);

    if (customer is null)
    {
        Console.WriteLine($"❌ No customer found with ID {customerId}.");
        return;
    }

    // US-012: CustomerProfile retrieved via explicit loading
    await context.Entry(customer).Reference(c => c.Profile).LoadAsync();

    Console.Write("New Address:     ");
    var address = Console.ReadLine();
    Console.Write("New City:        ");
    var city = Console.ReadLine();
    Console.Write("New PostalCode:  ");
    var postal = Console.ReadLine();

    // US-012: Upsert pattern — create profile if it doesn't exist
    if (customer.Profile is null)
    {
        customer.Profile = new CustomerProfile
        {
            CustomerId  = customer.CustomerId,
            Address     = address,
            City        = city,
            PostalCode  = postal
        };
        context.CustomerProfiles.Add(customer.Profile);
        Console.WriteLine("ℹ️  Profile did not exist — creating new profile.");
    }
    else
    {
        customer.Profile.Address    = address;
        customer.Profile.City       = city;
        customer.Profile.PostalCode = postal;

        // Concept Map: Update / UpdateRange — explicitly mark entity as Modified
        context.Update(customer.Profile);
        // UpdateRange example: context.CustomerProfiles.UpdateRange(listOfProfiles);
    }

    // US-012: SaveChanges called after update
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Address updated successfully.");
}

// ============================================================
//  SECTION 3 — Product Catalog
// ============================================================
async Task ProductMenu()
{
    Console.Clear();
    Console.WriteLine("=== Product Catalog ===");
    Console.WriteLine("1. US-020 — Browse all active products");
    Console.WriteLine("2. US-021 — Search products");
    Console.WriteLine("3. US-022 — View product details");
    Console.WriteLine("4. US-023 — Top 5 highest-rated products");
    Console.WriteLine("5. US-024 — Deactivate out-of-stock products");
    Console.Write("Choose: ");

    switch (Console.ReadLine())
    {
        case "1": await US020_BrowseActiveProducts(); break;
        case "2": await US021_SearchProducts(); break;
        case "3": await US022_ViewProductDetails(); break;
        case "4": await US023_Top5RatedProducts(); break;
        case "5": await US024_DeactivateOutOfStock(); break;
    }
    Pause();
}

// US-020: Browse all active products
// Acceptance: global filter on IsActive auto-applied, eager load Category,
//             OrderBy Price, Select projection, AsNoTracking
async Task US020_BrowseActiveProducts()
{
    Console.WriteLine("\n--- US-020: Active Products ---");
    Console.WriteLine("(Global query filter IsActive=true applied automatically)\n");

    // Concept Map: Find() — find a single entity by primary key
    Console.Write("Enter a Product ID to find directly (or 0 to skip): ");
    if (int.TryParse(Console.ReadLine(), out int findId) && findId > 0)
    {
        // Find() uses the identity map (cache) first, then hits DB — fastest PK lookup
        var found = await context.Products.FindAsync(findId);
        if (found is not null)
            Console.WriteLine($"  Find({findId}) → {found.Name} | IsActive: {found.IsActive}\n");
        else
            Console.WriteLine($"  Find({findId}) → not found.\n");
    }

    // US-020: AsNoTracking — read-only query
    // Global filter on IsActive=true is applied automatically by EF
    // Eager loading: Category
    // Projection: only Name, Price, CategoryName
    // OrderBy Price ascending
    var products = await context.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .OrderBy(p => p.Price)
        .Select(p => new
        {
            p.Name,
            p.Price,
            CategoryName = p.Category.Name
        })
        .ToListAsync();

    if (!products.Any())
    {
        Console.WriteLine("No active products found.");
        return;
    }

    Console.WriteLine($"{"Name",-35} {"Price",12} {"Category",-20}");
    Console.WriteLine(new string('-', 70));

    foreach (var p in products)
        Console.WriteLine($"{p.Name,-35} {p.Price,12:C} {p.CategoryName,-20}");

    Console.WriteLine($"\nTotal: {products.Count} active product(s).");
}

// US-021: Search products by name or category
// Acceptance: Where + string.Contains, join/nav for category, Any() check
async Task US021_SearchProducts()
{
    Console.WriteLine("\n--- US-021: Search Products ---");
    Console.Write("Keyword (name or category): ");
    var keyword = Console.ReadLine() ?? string.Empty;

    // US-021: Where() with string.Contains() for name search
    // Category filter uses navigation property
    var query = context.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .Where(p => p.Name.Contains(keyword) || p.Category.Name.Contains(keyword));

    // US-021: Any() to check if results exist before displaying
    if (!await query.AnyAsync())
    {
        Console.WriteLine($"No products found matching '{keyword}'.");
        return;
    }

    var results = await query
        .OrderBy(p => p.Name)
        .Select(p => new { p.Name, p.Price, CategoryName = p.Category.Name, p.SKU })
        .ToListAsync();

    Console.WriteLine($"\nResults for '{keyword}':\n");
    Console.WriteLine($"{"Name",-35} {"SKU",-20} {"Price",10} {"Category",-20}");
    Console.WriteLine(new string('-', 88));

    foreach (var p in results)
        Console.WriteLine($"{p.Name,-35} {p.SKU,-20} {p.Price,10:C} {p.CategoryName,-20}");
}

// US-022: View product details with tags and reviews
// Acceptance: ThenInclude(pt => pt.Tag), Average rating, Count
async Task US022_ViewProductDetails()
{
    Console.WriteLine("\n--- US-022: Product Details ---");
    Console.Write("Product ID: ");

    if (!int.TryParse(Console.ReadLine(), out int productId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    // US-022: Eager loading — ProductTags → Tag, and Reviews
    var product = await context.Products
        .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
        .Include(p => p.Reviews)
        .SingleOrDefaultAsync(p => p.ProductId == productId);

    if (product is null)
    {
        Console.WriteLine($"No product found with ID {productId}.");
        return;
    }

    Console.WriteLine($"\n📦 {product.Name}  (SKU: {product.SKU})");
    Console.WriteLine($"   Price:        {product.Price:C}");
    Console.WriteLine($"   Stock:        {product.StockQuantity}");
    Console.WriteLine($"   Display Name: {product.DisplayName}");

    // US-022: Average rating computed with .Average()
    // US-022: Total count with .Count()
    var reviewCount  = product.Reviews.Count;
    var avgRating    = reviewCount > 0
        ? product.Reviews.Average(r => r.Rating)
        : 0.0;

    Console.WriteLine($"   Reviews:      {reviewCount} | Avg Rating: {avgRating:F1} ⭐");

    var tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList();
    Console.WriteLine($"   Tags:         {(tags.Any() ? string.Join(", ", tags) : "None")}");

    if (reviewCount > 0)
    {
        Console.WriteLine("\n💬 Reviews:");
        foreach (var r in product.Reviews)
            Console.WriteLine($"   ⭐{r.Rating} — {r.Comment ?? "No comment"} ({r.CreatedAt:yyyy-MM-dd})");
    }
}

// US-023: Top 5 highest-rated products
// Acceptance: GroupBy ProductId, average rating, OrderByDescending, Take(5), projection
async Task US023_Top5RatedProducts()
{
    Console.WriteLine("\n--- US-023: Top 5 Highest-Rated Products ---");

    // Concept Map: Max / Min — highest and lowest rating ever given
    var maxRating = await context.Reviews.MaxAsync(r => r.Rating);
    var minRating = await context.Reviews.MinAsync(r => r.Rating);
    Console.WriteLine($"Highest rating ever: {maxRating} ⭐ | Lowest rating ever: {minRating} ⭐\n");

    // US-023: GroupBy on ProductId, compute average rating
    // OrderByDescending on average rating, Take(5)
    var top5 = await context.Reviews
        .GroupBy(r => r.ProductId)
        .Select(g => new
        {
            ProductId     = g.Key,
            AverageRating = g.Average(r => r.Rating)
        })
        .OrderByDescending(x => x.AverageRating)
        .Take(5)
        .Join(
            context.Products,
            r => r.ProductId,
            p => p.ProductId,
            (r, p) => new
            {
                ProductName   = p.Name,
                r.AverageRating
            })
        .ToListAsync();

    if (!top5.Any())
    {
        Console.WriteLine("No reviews yet.");
        return;
    }

    Console.WriteLine($"\n{"#",-4} {"Product",-35} {"Avg Rating",10}");
    Console.WriteLine(new string('-', 52));

    int rank = 1;
    foreach (var item in top5)
        Console.WriteLine($"{rank++,-4} {item.ProductName,-35} {item.AverageRating,10:F2} ⭐");
}

// US-024: Deactivate all out-of-stock products
// Acceptance: EF7 ExecuteUpdateAsync (bulk update, no tracking needed)
async Task US024_DeactivateOutOfStock()
{
    Console.WriteLine("\n--- US-024: Deactivate Out-of-Stock Products ---");

    // Count before to show the user what will happen
    // IgnoreQueryFilters because we want to see inactive ones too (not relevant here
    // but the global filter would skip already-inactive products)
    int toUpdate = await context.Products
        .IgnoreQueryFilters()
        .CountAsync(p => p.StockQuantity == 0 && p.IsActive);

    Console.WriteLine($"Products with StockQuantity=0 and IsActive=true: {toUpdate}");

    if (toUpdate == 0)
    {
        Console.WriteLine("Nothing to update.");
        return;
    }

    // US-024: EF7 ExecuteUpdateAsync — bulk update, no individual entity tracking
    int affected = await context.Products
        .IgnoreQueryFilters()
        .Where(p => p.StockQuantity == 0 && p.IsActive)
        .ExecuteUpdateAsync(setters =>
            setters.SetProperty(p => p.IsActive, false));

    // US-024: Verify with a count query after the update
    int remaining = await context.Products
        .IgnoreQueryFilters()
        .CountAsync(p => p.StockQuantity == 0 && p.IsActive);

    Console.WriteLine($"✅ Updated {affected} product(s) to IsActive=false.");
    Console.WriteLine($"   Remaining out-of-stock but active: {remaining}");
}

// ============================================================
//  SECTION 4 — Order Processing
// ============================================================
async Task OrderMenu()
{
    Console.Clear();
    Console.WriteLine("=== Order Processing ===");
    Console.WriteLine("1. US-030 — Place a new order");
    Console.WriteLine("2. US-031 — View order history");
    Console.WriteLine("3. US-032 — Cancel a pending order");
    Console.WriteLine("4. US-033 — Monthly revenue report");
    Console.WriteLine("5. US-034 — Pending orders (raw SQL)");
    Console.Write("Choose: ");

    switch (Console.ReadLine())
    {
        case "1": await US030_PlaceOrder(); break;
        case "2": await US031_ViewOrderHistory(); break;
        case "3": await US032_CancelOrder(); break;
        case "4": await US033_MonthlyRevenue(); break;
        case "5": await US034_PendingOrdersRawSql(); break;
    }
    Pause();
}

// US-030: Place a new order
// Acceptance: Order + OrderItems in transaction, decrement stock,
//             compute TotalAmount, Payment created as Pending
async Task US030_PlaceOrder()
{
    Console.WriteLine("\n--- US-030: Place New Order ---");
    Console.Write("Customer ID: ");

    if (!int.TryParse(Console.ReadLine(), out int customerId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    var customer = await context.Customers.FindAsync(customerId);
    if (customer is null)
    {
        Console.WriteLine("Customer not found.");
        return;
    }

    // Collect order items
    var orderLines = new List<(int ProductId, int Quantity)>();
    Console.WriteLine("Enter products (press Enter with empty Product ID to finish):");

    while (true)
    {
        Console.Write("  Product ID: ");
        var pidInput = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(pidInput)) break;

        if (!int.TryParse(pidInput, out int pid)) continue;

        Console.Write("  Quantity:   ");
        if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0) continue;

        orderLines.Add((pid, qty));
    }

    if (!orderLines.Any())
    {
        Console.WriteLine("No items entered.");
        return;
    }

    // US-030: Order + OrderItems created inside a transaction
    await using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        decimal total = 0;
        var orderItems = new List<OrderItem>();

        foreach (var (productId, quantity) in orderLines)
        {
            // IgnoreQueryFilters to allow ordering inactive products if needed
            var product = await context.Products
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(p => p.ProductId == productId);

            if (product is null)
            {
                Console.WriteLine($"⚠️  Product ID {productId} not found — skipped.");
                continue;
            }

            if (product.StockQuantity < quantity)
            {
                Console.WriteLine($"⚠️  Insufficient stock for '{product.Name}' (available: {product.StockQuantity}) — skipped.");
                continue;
            }

            // US-030: StockQuantity decremented for each product
            product.StockQuantity -= quantity;

            var unitPrice = product.Price;
            // US-030: TotalAmount computed as sum of (UnitPrice * Quantity)
            total += unitPrice * quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = product.ProductId,
                Quantity  = quantity,
                UnitPrice = unitPrice
            });
        }

        if (!orderItems.Any())
        {
            await transaction.RollbackAsync();
            Console.WriteLine("❌ No valid items — order cancelled.");
            return;
        }

        var order = new Order
        {
            CustomerId  = customerId,
            Status      = OrderStatus.Pending,
            TotalAmount = total,
            OrderItems  = orderItems
            // PlacedAt set automatically by HasDefaultValueSql("GETUTCDATE()")
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(); // generates OrderId

        // US-030: Payment created as Pending in the same transaction
        var payment = new Payment
        {
            OrderId = order.OrderId,
            Method  = PaymentMethod.CreditCard,
            Status  = PaymentStatus.Pending,
            Amount  = total
        };

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        await transaction.CommitAsync();

        Console.WriteLine($"\n✅ Order #{order.OrderId} placed successfully!");
        Console.WriteLine($"   Total: {total:C} | Status: {order.Status}");
        Console.WriteLine($"   Payment #{payment.PaymentId} created as Pending.");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        Console.WriteLine($"❌ Order failed and rolled back: {ex.Message}");
    }
}

// US-031: View order history
// Acceptance: Where by customerId, eager load OrderItems + Payment,
//             OrderByDescending PlacedAt, FirstOrDefault for most recent
async Task US031_ViewOrderHistory()
{
    Console.WriteLine("\n--- US-031: Order History ---");
    Console.Write("Customer ID: ");

    if (!int.TryParse(Console.ReadLine(), out int customerId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    // US-031: Where filters by customerId
    // Eager loading: OrderItems and Payment
    // OrderByDescending PlacedAt
    var orders = await context.Orders
        .Where(o => o.CustomerId == customerId)
        .Include(o => o.OrderItems)
        .Include(o => o.Payment)
        .OrderByDescending(o => o.PlacedAt)
        .ToListAsync();

    if (!orders.Any())
    {
        Console.WriteLine("No orders found for this customer.");
        return;
    }

    // US-031: FirstOrDefault returns the most recent order
    var mostRecent = orders.FirstOrDefault();
    Console.WriteLine($"\nMost recent order: #{mostRecent!.OrderId} on {mostRecent.PlacedAt:yyyy-MM-dd}\n");

    foreach (var order in orders)
    {
        Console.WriteLine($"📦 Order #{order.OrderId} | {order.Status} | {order.TotalAmount:C} | {order.PlacedAt:yyyy-MM-dd}");
        Console.WriteLine($"   Items: {order.OrderItems.Count} | Payment: {order.Payment?.Status.ToString() ?? "N/A"}");
    }
}

// US-032: Cancel a pending order
// Acceptance: Single/SingleOrDefault finds the order, status → Cancelled,
//             restore StockQuantity in transaction, Payment → Refunded
async Task US032_CancelOrder()
{
    Console.WriteLine("\n--- US-032: Cancel Pending Order ---");
    Console.Write("Order ID: ");

    if (!int.TryParse(Console.ReadLine(), out int orderId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    // US-032: SingleOrDefault finds the specific order
    var order = await context.Orders
        .Include(o => o.OrderItems)
        .Include(o => o.Payment)
        .SingleOrDefaultAsync(o => o.OrderId == orderId);

    if (order is null)
    {
        Console.WriteLine("Order not found.");
        return;
    }

    if (order.Status != OrderStatus.Pending)
    {
        Console.WriteLine($"❌ Cannot cancel order with status '{order.Status}'. Only Pending orders can be cancelled.");
        return;
    }

    await using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        // US-032: Status updated to Cancelled using Update()
        order.Status = OrderStatus.Cancelled;
        context.Update(order);  // explicit Update() call as required by spec

        // US-032: StockQuantity restored for each OrderItem in the transaction
        // Concept Map: Remove/RemoveRange — delete related data
        foreach (var item in order.OrderItems)
        {
            var product = await context.Products
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(p => p.ProductId == item.ProductId);

            if (product is not null)
                product.StockQuantity += item.Quantity;
        }

        // Concept Map: Delete Related Data — demonstrates RemoveRange on OrderItems
        // (we restore stock and keep items for audit trail but show RemoveRange usage)
        // Uncomment below line to actually delete items:
        // context.OrderItems.RemoveRange(order.OrderItems);

        // US-032: Payment status updated to Refunded
        if (order.Payment is not null)
        {
            order.Payment.Status = PaymentStatus.Refunded;
            context.Update(order.Payment);
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        Console.WriteLine($"✅ Order #{orderId} cancelled. Stock restored. Payment refunded.");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        Console.WriteLine($"❌ Cancellation failed and rolled back: {ex.Message}");
    }
}

// US-033: Monthly revenue report
// Acceptance: GroupBy PlacedAt.Month, Sum TotalAmount, Where Delivered,
//             OrderBy group key
async Task US033_MonthlyRevenue()
{
    Console.WriteLine("\n--- US-033: Monthly Revenue Report (Current Year) ---");

    int currentYear = DateTime.UtcNow.Year;

    // US-033: Where filters Delivered orders only
    // GroupBy month, Sum TotalAmount, OrderBy month
    var report = await context.Orders
        .Where(o => o.Status == OrderStatus.Delivered && o.PlacedAt.Year == currentYear)
        .GroupBy(o => o.PlacedAt.Month)
        .Select(g => new
        {
            Month   = g.Key,
            Revenue = g.Sum(o => o.TotalAmount)
        })
        .OrderBy(g => g.Month)
        .ToListAsync();

    if (!report.Any())
    {
        Console.WriteLine("No delivered orders this year.");
        return;
    }

    Console.WriteLine($"\n{"Month",-12} {"Revenue",15}");
    Console.WriteLine(new string('-', 30));

    foreach (var row in report)
    {
        var monthName = new DateTime(currentYear, row.Month, 1).ToString("MMMM");
        Console.WriteLine($"{monthName,-12} {row.Revenue,15:C}");
    }

    Console.WriteLine(new string('-', 30));
    Console.WriteLine($"{"TOTAL",-12} {report.Sum(r => r.Revenue),15:C}");
}

// US-034: View pending orders using raw SQL
// Acceptance: FromSqlRaw or FromSqlInterpolated, stored procedure GetPendingOrders
async Task US034_PendingOrdersRawSql()
{
    Console.WriteLine("\n--- US-034: Pending Orders via Raw SQL ---");

    // US-034 Part 1: FromSqlRaw — demonstrates raw SQL mapped to entities
    Console.WriteLine("\n[Part 1] Using FromSqlRaw:");
    try
    {
        var pendingOrders = await context.Orders
            .FromSqlRaw("SELECT * FROM shop.Orders WHERE Status = 'Pending'")
            .Include(o => o.Customer)
            .ToListAsync();

        if (!pendingOrders.Any())
        {
            Console.WriteLine("No pending orders.");
        }
        else
        {
            foreach (var o in pendingOrders)
                Console.WriteLine($"  Order #{o.OrderId} | Customer: {o.Customer.FullName} | {o.TotalAmount:C}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Raw SQL error: {ex.Message}");
    }

    // US-034 Part 2: Stored procedure (must be created via migration)
    Console.WriteLine("\n[Part 2] Using stored procedure GetPendingOrders:");
    Console.WriteLine("  Stored procedure is created via migration. Example migration SQL:");
    Console.WriteLine();
    Console.WriteLine("  CREATE PROCEDURE shop.GetPendingOrders AS");
    Console.WriteLine("  BEGIN");
    Console.WriteLine("      SELECT * FROM shop.Orders WHERE Status = 'Pending';");
    Console.WriteLine("  END");
    Console.WriteLine();
    Console.WriteLine("  Call in EF Core:");
    Console.WriteLine("  context.Orders.FromSqlRaw(\"EXEC shop.GetPendingOrders\").ToListAsync();");

    try
    {
        // Attempt to call the stored procedure if it exists
        var spResults = await context.Orders
            .FromSqlRaw("EXEC shop.GetPendingOrders")
            .ToListAsync();

        Console.WriteLine($"\n  Stored procedure returned {spResults.Count} result(s).");
    }
    catch
    {
        Console.WriteLine("\n  (Stored procedure not yet created — run the migration first.)");
    }
}

// ============================================================
//  SECTION 5 — Payments & Discounts
// ============================================================
async Task PaymentsMenu()
{
    Console.Clear();
    Console.WriteLine("=== Payments & Discounts ===");
    Console.WriteLine("1. US-040 — Apply discount code");
    Console.WriteLine("2. US-041 — Bulk delete expired discounts");
    Console.Write("Choose: ");

    switch (Console.ReadLine())
    {
        case "1": await US040_ApplyDiscount(); break;
        case "2": await US041_BulkDeleteExpiredDiscounts(); break;
    }
    Pause();
}

// US-040: Apply discount code at checkout
// Acceptance: SingleOrDefault by code, validate IsActive + ExpiresAt + MaxUses,
//             apply Percentage to TotalAmount, increment CurrentUses
async Task US040_ApplyDiscount()
{
    Console.WriteLine("\n--- US-040: Apply Discount Code ---");
    Console.Write("Order ID:      ");
    if (!int.TryParse(Console.ReadLine(), out int orderId))
    {
        Console.WriteLine("Invalid Order ID.");
        return;
    }

    Console.Write("Discount Code: ");
    var code = Console.ReadLine() ?? string.Empty;

    var order = await context.Orders.FindAsync(orderId);
    if (order is null)
    {
        Console.WriteLine("Order not found.");
        return;
    }

    // US-040: Discount found using SingleOrDefault(d => d.Code == code)
    var discount = await context.Discounts
        .SingleOrDefaultAsync(d => d.Code == code);

    if (discount is null)
    {
        Console.WriteLine("❌ Discount code not found.");
        return;
    }

    // US-040: Validate IsActive
    if (!discount.IsActive)
    {
        Console.WriteLine("❌ Discount code is not active.");
        return;
    }

    // US-040: Validate ExpiresAt
    if (discount.ExpiresAt < DateTime.UtcNow)
    {
        Console.WriteLine("❌ Discount code has expired.");
        return;
    }

    // US-040: Validate CurrentUses < MaxUses
    if (discount.CurrentUses >= discount.MaxUses)
    {
        Console.WriteLine("❌ Discount code has reached its maximum usage limit.");
        return;
    }

    // US-040: Apply Percentage to TotalAmount and increment CurrentUses
    decimal originalTotal   = order.TotalAmount;
    decimal discountAmount  = originalTotal * (discount.Percentage / 100);
    order.TotalAmount       = originalTotal - discountAmount;
    discount.CurrentUses++;

    await context.SaveChangesAsync();

    Console.WriteLine($"✅ Discount applied!");
    Console.WriteLine($"   Code:          {discount.Code} ({discount.Percentage}% off)");
    Console.WriteLine($"   Original:      {originalTotal:C}");
    Console.WriteLine($"   Discount:     -{discountAmount:C}");
    Console.WriteLine($"   New Total:     {order.TotalAmount:C}");
    Console.WriteLine($"   Uses:          {discount.CurrentUses}/{discount.MaxUses}");
}

// US-041: Bulk delete expired discounts
// Acceptance: EF7 ExecuteDeleteAsync, no entity tracking, print deleted count
async Task US041_BulkDeleteExpiredDiscounts()
{
    Console.WriteLine("\n--- US-041: Bulk Delete Expired Discounts ---");

    // Show what will be deleted
    int toDelete = await context.Discounts
        .CountAsync(d => d.ExpiresAt < DateTime.UtcNow || !d.IsActive);

    Console.WriteLine($"Expired or inactive discounts found: {toDelete}");

    if (toDelete == 0)
    {
        Console.WriteLine("Nothing to delete.");
        return;
    }

    // US-041: EF7 ExecuteDeleteAsync — no entities tracked or loaded
    int deleted = await context.Discounts
        .Where(d => d.ExpiresAt < DateTime.UtcNow || !d.IsActive)
        .ExecuteDeleteAsync();

    // US-041: Print count of deleted rows
    Console.WriteLine($"✅ Deleted {deleted} expired/inactive discount(s). No entities were loaded into memory.");
}

// ============================================================
//  SECTION 6 — Advanced Queries & Loading
// ============================================================
async Task AdvancedMenu()
{
    Console.Clear();
    Console.WriteLine("=== Advanced Queries & Loading ===");
    Console.WriteLine("1. US-050 — Lazy loading demo");
    Console.WriteLine("2. US-051 — Split queries demo");
    Console.WriteLine("3. US-052 — Customers with no orders");
    Console.WriteLine("4. US-053 — Products ranked by quantity sold");
    Console.WriteLine("5. Concept — Distinct() unique tag names");
    Console.WriteLine("6. Concept — MaxBy/MinBy most expensive product per category");
    Console.Write("Choose: ");

    switch (Console.ReadLine())
    {
        case "1": await US050_LazyLoadingDemo(); break;
        case "2": await US051_SplitQueriesDemo(); break;
        case "3": await US052_CustomersWithNoOrders(); break;
        case "4": await US053_ProductsByQuantitySold(); break;
        case "5": await Concept_Distinct(); break;
        case "6": await Concept_MaxByMinBy(); break;
    }
    Pause();
}

// US-050: Lazy loading demo
// Acceptance: UseLazyLoadingProxies() configured in DbContext (virtual nav props),
//             Microsoft.EntityFrameworkCore.Proxies installed,
//             console proves queries fire on navigation access
async Task US050_LazyLoadingDemo()
{
    Console.WriteLine("\n--- US-050: Lazy Loading Demo ---");
    Console.WriteLine("UseLazyLoadingProxies() is configured in AppDbContext.OnConfiguring().");
    Console.WriteLine("Navigation properties marked 'virtual' trigger DB query on access.\n");

    Console.Write("Product ID to load (lazy): ");
    if (!int.TryParse(Console.ReadLine(), out int productId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    // Load product WITHOUT including Reviews
    var product = await context.Products
        .IgnoreQueryFilters()
        .SingleOrDefaultAsync(p => p.ProductId == productId);

    if (product is null)
    {
        Console.WriteLine("Product not found.");
        return;
    }

    Console.WriteLine($"Product loaded: {product.Name}");
    Console.WriteLine("Reviews NOT included in query — accessing Reviews nav property now...");

    // US-050: Navigation access triggers a new DB query (lazy loading via proxy)
    // This fires a second SELECT automatically
    int reviewCount = product.Reviews.Count;
    Console.WriteLine($"✅ Reviews loaded lazily — count: {reviewCount}");
    Console.WriteLine("(A separate SQL SELECT was issued when product.Reviews was accessed)");
}

// US-051: Split queries demo
// Acceptance: AsSplitQuery(), multiple SELECTs, no cartesian explosion
async Task US051_SplitQueriesDemo()
{
    Console.WriteLine("\n--- US-051: Split Queries Demo ---");
    Console.Write("Customer ID: ");

    if (!int.TryParse(Console.ReadLine(), out int customerId))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    Console.WriteLine("\nLoading customer with Orders, OrderItems, and Reviews using AsSplitQuery()...");

    // US-051: AsSplitQuery() prevents cartesian explosion
    // Multiple SQL SELECT statements are generated instead of one giant JOIN
    var customer = await context.Customers
        .AsSplitQuery()
        .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems)
        .Include(c => c.Reviews)
        .SingleOrDefaultAsync(c => c.CustomerId == customerId);

    if (customer is null)
    {
        Console.WriteLine("Customer not found.");
        return;
    }

    Console.WriteLine($"✅ Customer loaded: {customer.FullName}");
    Console.WriteLine($"   Orders:  {customer.Orders.Count}");
    Console.WriteLine($"   Items:   {customer.Orders.Sum(o => o.OrderItems.Count)}");
    Console.WriteLine($"   Reviews: {customer.Reviews.Count}");
    Console.WriteLine("\n(EF Core issued separate SELECT statements — no duplicate rows from JOIN explosion)");
}

// US-052: Find customers with no orders
// Acceptance: GroupJoin() or Where + !Any(), returns FullName and Email
async Task US052_CustomersWithNoOrders()
{
    Console.WriteLine("\n--- US-052: Customers With No Orders ---");

    // US-052: Left join using GroupJoin — demonstrates the GroupJoin lesson
    var allCustomers = context.Customers.AsNoTracking();
    var allOrders    = context.Orders.AsNoTracking();

    // GroupJoin = left outer join: keeps all customers, matches zero or more orders
    var result = await allCustomers
        .GroupJoin(
            allOrders,
            customer => customer.CustomerId,
            order    => order.CustomerId,
            (customer, orders) => new { customer, orders })
        .Where(x => !x.orders.Any())   // only customers with no orders
        .Select(x => new
        {
            x.customer.FullName,
            x.customer.Email
        })
        .ToListAsync();

    // Alternative approach using Where + !Any():
    // var result = await context.Customers
    //     .Where(c => !context.Orders.Any(o => o.CustomerId == c.CustomerId))
    //     .Select(c => new { c.FullName, c.Email })
    //     .ToListAsync();

    if (!result.Any())
    {
        Console.WriteLine("All customers have placed at least one order.");
        return;
    }

    Console.WriteLine($"\nCustomers with no orders ({result.Count}):\n");
    Console.WriteLine($"{"Full Name",-35} {"Email",-40}");
    Console.WriteLine(new string('-', 77));

    foreach (var c in result)
        Console.WriteLine($"{c.FullName,-35} {c.Email,-40}");
}

// US-053: Products ranked by total quantity sold
// Acceptance: Inner Join between Products and OrderItems using Join(),
//             GroupBy ProductId, Sum Quantity, Select projection, OrderByDescending
async Task US053_ProductsByQuantitySold()
{
    Console.WriteLine("\n--- US-053: Products Ranked by Quantity Sold ---");

    // US-053: Inner Join using Join() between Products and OrderItems
    // GroupBy ProductId, Sum Quantity
    var ranked = await context.Products
        .IgnoreQueryFilters()
        .Join(
            context.OrderItems,
            product   => product.ProductId,
            orderItem => orderItem.ProductId,
            (product, orderItem) => new { product.Name, orderItem.Quantity })
        .GroupBy(x => x.Name)
        .Select(g => new
        {
            ProductName = g.Key,
            TotalSold   = g.Sum(x => x.Quantity)
        })
        // US-053: OrderByDescending on TotalSold
        .OrderByDescending(x => x.TotalSold)
        .ToListAsync();

    if (!ranked.Any())
    {
        Console.WriteLine("No sales data yet.");
        return;
    }

    Console.WriteLine($"\n{"#",-4} {"Product",-35} {"Total Sold",12}");
    Console.WriteLine(new string('-', 54));

    int rank = 1;
    foreach (var item in ranked)
        Console.WriteLine($"{rank++,-4} {item.ProductName,-35} {item.TotalSold,12}");
}

// ============================================================
//  Concept Map: Distinct() — unique tag names on filtered query
// ============================================================
async Task Concept_Distinct()
{
    Console.WriteLine("\n--- Concept: Distinct() — Unique Tag Names ---");

    // Distinct() on tag names across all products
    // Demonstrates: get every unique tag name in use, without duplicates
    var distinctTags = await context.ProductTags
        .AsNoTracking()
        .Include(pt => pt.Tag)
        .Select(pt => pt.Tag.Name)
        .Distinct()
        .OrderBy(name => name)
        .ToListAsync();

    if (!distinctTags.Any())
    {
        Console.WriteLine("No tags found.");
        return;
    }

    Console.WriteLine($"\nAll distinct tag names ({distinctTags.Count}):");
    foreach (var tag in distinctTags)
        Console.WriteLine($"  • {tag}");
}

// ============================================================
//  Concept Map: MaxBy/MinBy — most expensive product in category
// ============================================================
async Task Concept_MaxByMinBy()
{
    Console.WriteLine("\n--- Concept: MaxBy / MinBy — Most & Least Expensive Product per Category ---");

    // Load all active products with their categories
    var products = await context.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .ToListAsync();

    if (!products.Any())
    {
        Console.WriteLine("No products found.");
        return;
    }

    // Group in memory and use MaxBy / MinBy (LINQ to Objects)
    var grouped = products
        .GroupBy(p => p.Category.Name)
        .OrderBy(g => g.Key);

    Console.WriteLine($"\n{"Category",-25} {"Most Expensive",-35} {"Price",12}   {"Cheapest",-35} {"Price",12}");
    Console.WriteLine(new string('-', 125));

    foreach (var group in grouped)
    {
        // Concept Map: MaxBy / MinBy
        var mostExpensive = group.MaxBy(p => p.Price)!;
        var cheapest      = group.MinBy(p => p.Price)!;

        Console.WriteLine(
            $"{group.Key,-25} {mostExpensive.Name,-35} {mostExpensive.Price,12:C}   " +
            $"{cheapest.Name,-35} {cheapest.Price,12:C}");
    }
}


void Pause()
{
    Console.WriteLine("\nPress any key to go back...");
    Console.ReadKey();
}
