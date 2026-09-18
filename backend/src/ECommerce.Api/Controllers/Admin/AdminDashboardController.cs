using ECommerce.Api.Common;
using ECommerce.Application.Admin;
using ECommerce.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers.Admin;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/dashboard")]
public class AdminDashboardController : ApiControllerBase
{
    private readonly IAdminService _adminService;

    public AdminDashboardController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> Get() => Ok(await _adminService.GetDashboardSummaryAsync());
}
