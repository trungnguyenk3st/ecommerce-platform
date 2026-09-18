using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? MaxUsageCount { get; set; }
    public int UsageCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsValidFor(decimal orderSubtotal, DateTime now)
    {
        if (!IsActive) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value < now) return false;
        if (MaxUsageCount.HasValue && UsageCount >= MaxUsageCount.Value) return false;
        if (MinOrderAmount.HasValue && orderSubtotal < MinOrderAmount.Value) return false;
        return true;
    }

    public decimal CalculateDiscount(decimal orderSubtotal)
    {
        var discount = DiscountType == DiscountType.Percentage
            ? orderSubtotal * (DiscountValue / 100m)
            : DiscountValue;

        if (MaxDiscountAmount.HasValue)
            discount = Math.Min(discount, MaxDiscountAmount.Value);

        return Math.Min(discount, orderSubtotal);
    }
}
