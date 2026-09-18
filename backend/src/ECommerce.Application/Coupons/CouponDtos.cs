using ECommerce.Domain.Enums;

namespace ECommerce.Application.Coupons;

public record CouponDto(
    int Id, string Code, DiscountType DiscountType, decimal DiscountValue,
    decimal? MinOrderAmount, decimal? MaxDiscountAmount, int? MaxUsageCount, int UsageCount,
    DateTime? ExpiresAt, bool IsActive);

public record CouponCreateUpdateRequest(
    string Code, DiscountType DiscountType, decimal DiscountValue,
    decimal? MinOrderAmount, decimal? MaxDiscountAmount, int? MaxUsageCount,
    DateTime? ExpiresAt, bool IsActive);

public record ValidateCouponRequest(string Code, decimal OrderSubtotal);
public record ValidateCouponResponse(bool IsValid, decimal DiscountAmount, string? Message);
