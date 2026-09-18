using ECommerce.Api.Common;
using ECommerce.Application.Admin;
using ECommerce.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers.Admin;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/customers")]
public class AdminCustomersController : ApiControllerBase
{
    private readonly IAdminService _adminService;

    public AdminCustomersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll() => Ok(await _adminService.GetCustomersAsync());

    public record SetLockRequest(bool Locked);

    [HttpPut("{userId}/lock")]
    public async Task<ActionResult<CustomerDto>> SetLock(string userId, SetLockRequest request)
        => Ok(await _adminService.SetCustomerLockAsync(userId, request.Locked));
}
