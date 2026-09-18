using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Orders;

public interface IOrderService
{
    Task<PlaceOrderResult> PlaceOrderAsync(string userId, PlaceOrderRequest request, string clientIpAddress);
    Task<PagedResult<OrderSummaryDto>> GetMyOrdersAsync(string userId, OrderQueryParams query);
    Task<OrderDto> GetMyOrderAsync(string userId, int orderId);
    Task<OrderDto> CancelMyOrderAsync(string userId, int orderId);

    Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(OrderQueryParams query);
    Task<OrderDto> GetOrderAsync(int orderId);
    Task<OrderDto> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request);

    Task<OrderDto> HandlePaymentCallbackAsync(string orderNumber, bool success, string transactionId, string rawResponse);
}
