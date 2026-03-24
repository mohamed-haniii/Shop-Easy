namespace ShopEasy.Models;

public class Review
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
}
