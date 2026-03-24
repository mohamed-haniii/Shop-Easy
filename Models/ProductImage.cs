namespace ShopEasy.Models;

public class ProductImage
{
    public int ProductImageId { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
}
