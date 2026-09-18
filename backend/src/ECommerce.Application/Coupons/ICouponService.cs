namespace ECommerce.Application.Coupons;

public interface ICouponService
{
    Task<List<CouponDto>> GetAllAsync();
    Task<CouponDto> CreateAsync(CouponCreateUpdateRequest request);
    Task<CouponDto> UpdateAsync(int id, CouponCreateUpdateRequest request);
    Task DeleteAsync(int id);
    Task<ValidateCouponResponse> ValidateAsync(ValidateCouponRequest request);
}
