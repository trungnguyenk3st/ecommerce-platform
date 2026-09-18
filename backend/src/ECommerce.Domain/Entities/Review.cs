using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Review : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;

    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; } = true;
}
