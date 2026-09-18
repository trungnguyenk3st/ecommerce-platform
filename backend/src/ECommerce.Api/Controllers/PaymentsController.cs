using ECommerce.Api.Common;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[AllowAnonymous]
[Route("api/payments")]
public class PaymentsController : ApiControllerBase
{
    private readonly IVnPayService _vnPayService;
    private readonly IOrderService _orderService;

    public PaymentsController(IVnPayService vnPayService, IOrderService orderService)
    {
        _vnPayService = vnPayService;
        _orderService = orderService;
    }

    /// <summary>Browser redirect target after the customer finishes on VNPay's hosted checkout page.</summary>
    [HttpGet("vnpay/return")]
    public async Task<IActionResult> VnPayReturn()
    {
        var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
        var result = _vnPayService.ValidateCallback(queryParams);

        if (!result.IsValidSignature)
            return BadRequest(new { message = "Invalid payment signature." });

        var order = await _orderService.HandlePaymentCallbackAsync(
            result.OrderNumber, result.IsSuccess, result.TransactionId, Request.QueryString.ToString());

        return Ok(order);
    }

    /// <summary>Server-to-server Instant Payment Notification callback from VNPay.</summary>
    [HttpGet("vnpay/ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        var queryParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
        var result = _vnPayService.ValidateCallback(queryParams);

        if (!result.IsValidSignature)
            return Ok(new { RspCode = "97", Message = "Invalid signature" });

        await _orderService.HandlePaymentCallbackAsync(
            result.OrderNumber, result.IsSuccess, result.TransactionId, Request.QueryString.ToString());

        return Ok(new { RspCode = "00", Message = "Confirm Success" });
    }
}
