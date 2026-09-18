using ECommerce.Api.Common;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize]
public class OrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<PlaceOrderResult>> PlaceOrder(PlaceOrderRequest request)
        => Ok(await _orderService.PlaceOrderAsync(CurrentUserId, request, ClientIpAddress));

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetMyOrders([FromQuery] OrderQueryParams query)
        => Ok(await _orderService.GetMyOrdersAsync(CurrentUserId, query));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetMyOrder(int id) => Ok(await _orderService.GetMyOrderAsync(CurrentUserId, id));

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(int id) => Ok(await _orderService.CancelMyOrderAsync(CurrentUserId, id));
}
