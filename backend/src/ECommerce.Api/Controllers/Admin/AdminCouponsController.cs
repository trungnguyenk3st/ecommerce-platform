using ECommerce.Api.Common;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Coupons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers.Admin;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/coupons")]
public class AdminCouponsController : ApiControllerBase
{
    private readonly ICouponService _couponService;

    public AdminCouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CouponDto>>> GetAll() => Ok(await _couponService.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create(CouponCreateUpdateRequest request)
        => Ok(await _couponService.CreateAsync(request));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CouponDto>> Update(int id, CouponCreateUpdateRequest request)
        => Ok(await _couponService.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _couponService.DeleteAsync(id);
        return NoContent();
    }
}
