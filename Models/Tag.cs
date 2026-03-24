namespace ShopEasy.Models;

public class Tag
{
    public int TagId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation Properties
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
