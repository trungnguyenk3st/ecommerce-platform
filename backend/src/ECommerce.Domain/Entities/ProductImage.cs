using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductImage : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
