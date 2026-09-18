using ECommerce.Api.Common;
using ECommerce.Application.Coupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize]
public class CouponsController : ApiControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ValidateCouponResponse>> Validate(ValidateCouponRequest request)
        => Ok(await _couponService.ValidateAsync(request));
}
