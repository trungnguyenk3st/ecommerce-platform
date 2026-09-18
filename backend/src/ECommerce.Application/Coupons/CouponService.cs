using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Coupons;

public class CouponService : ICouponService
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CouponCreateUpdateRequest> _validator;

    public CouponService(IApplicationDbContext db, IValidator<CouponCreateUpdateRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<List<CouponDto>> GetAllAsync()
    {
        var coupons = await _db.Coupons.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();
        return coupons.Select(ToDto).ToList();
    }

    public async Task<CouponDto> CreateAsync(CouponCreateUpdateRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var code = request.Code.Trim().ToUpperInvariant();
        var duplicate = await _db.Coupons.AnyAsync(c => c.Code == code);
        if (duplicate) throw new DomainException($"Coupon code '{code}' already exists.");

        var coupon = new Coupon
        {
            Code = code,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinOrderAmount = request.MinOrderAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MaxUsageCount = request.MaxUsageCount,
            ExpiresAt = request.ExpiresAt,
            IsActive = request.IsActive
        };

        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();
        return ToDto(coupon);
    }

    public async Task<CouponDto> UpdateAsync(int id, CouponCreateUpdateRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException(nameof(Coupon), id);

        var code = request.Code.Trim().ToUpperInvariant();
        var duplicate = await _db.Coupons.AnyAsync(c => c.Code == code && c.Id != id);
        if (duplicate) throw new DomainException($"Coupon code '{code}' already exists.");

        coupon.Code = code;
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.MinOrderAmount = request.MinOrderAmount;
        coupon.MaxDiscountAmount = request.MaxDiscountAmount;
        coupon.MaxUsageCount = request.MaxUsageCount;
        coupon.ExpiresAt = request.ExpiresAt;
        coupon.IsActive = request.IsActive;
        coupon.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(coupon);
    }

    public async Task DeleteAsync(int id)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException(nameof(Coupon), id);

        _db.Coupons.Remove(coupon);
        await _db.SaveChangesAsync();
    }

    public async Task<ValidateCouponResponse> ValidateAsync(ValidateCouponRequest request)
    {
        var coupon = await _db.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == request.Code.Trim().ToUpperInvariant());

        if (coupon is null)
            return new ValidateCouponResponse(false, 0, "Coupon code not found.");

        if (!coupon.IsValidFor(request.OrderSubtotal, DateTime.UtcNow))
            return new ValidateCouponResponse(false, 0, "Coupon is expired, exhausted, or the order does not meet its minimum amount.");

        return new ValidateCouponResponse(true, coupon.CalculateDiscount(request.OrderSubtotal), null);
    }

    private static CouponDto ToDto(Coupon c) => new(
        c.Id, c.Code, c.DiscountType, c.DiscountValue, c.MinOrderAmount, c.MaxDiscountAmount,
        c.MaxUsageCount, c.UsageCount, c.ExpiresAt, c.IsActive);
}
