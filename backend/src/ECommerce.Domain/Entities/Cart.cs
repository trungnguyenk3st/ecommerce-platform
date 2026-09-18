using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Cart : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}
