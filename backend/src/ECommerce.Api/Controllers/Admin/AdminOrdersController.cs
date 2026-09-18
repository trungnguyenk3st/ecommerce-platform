using ECommerce.Api.Common;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers.Admin;

[Authorize(Roles = Roles.Admin)]
[Route("api/admin/orders")]
public class AdminOrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    public AdminOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetAll([FromQuery] OrderQueryParams query)
        => Ok(await _orderService.GetAllOrdersAsync(query));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id) => Ok(await _orderService.GetOrderAsync(id));

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(int id, UpdateOrderStatusRequest request)
        => Ok(await _orderService.UpdateStatusAsync(id, request));
}
