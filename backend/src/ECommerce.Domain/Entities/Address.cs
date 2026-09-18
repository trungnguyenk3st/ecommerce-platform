using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Address : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Vietnam";
    public bool IsDefault { get; set; }
}
