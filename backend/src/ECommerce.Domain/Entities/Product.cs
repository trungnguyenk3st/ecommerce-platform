using ECommerce.Domain.Common;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public bool InStock => StockQuantity > 0;

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");
        if (StockQuantity < quantity) throw new DomainException($"Insufficient stock for product '{Name}'.");
        StockQuantity -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantity must be positive.");
        StockQuantity += quantity;
    }

    public void RecalculateRating()
    {
        var approved = Reviews.Where(r => r.IsApproved).ToList();
        ReviewCount = approved.Count;
        AverageRating = approved.Count == 0 ? 0 : Math.Round(approved.Average(r => r.Rating), 2);
    }
}
