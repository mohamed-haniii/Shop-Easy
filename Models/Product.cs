namespace ShopEasy.Models;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }

    // Computed column — set by DB, not by app code
    public string? DisplayName { get; set; }

    // Navigation Properties
    public virtual Category Category { get; set; } = null!;
    public virtual ProductImage? ProductImage { get; set; }
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
