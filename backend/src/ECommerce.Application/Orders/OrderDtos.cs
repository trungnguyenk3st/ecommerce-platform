using ECommerce.Domain.Enums;

namespace ECommerce.Application.Orders;

public record OrderItemDto(int ProductId, string ProductName, string? ProductImageUrl, decimal UnitPrice, int Quantity, decimal LineTotal);

public record OrderDto(
    int Id,
    string OrderNumber,
    OrderStatus Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal ShippingFee,
    decimal Total,
    string? CouponCode,
    string ShippingFullName,
    string ShippingPhone,
    string ShippingLine1,
    string? ShippingLine2,
    string ShippingCity,
    string ShippingProvince,
    string? ShippingPostalCode,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    string? CustomerNote,
    DateTime CreatedAt,
    List<OrderItemDto> Items);

public record OrderSummaryDto(
    int Id, string OrderNumber, OrderStatus Status, decimal Total, PaymentStatus PaymentStatus, DateTime CreatedAt, int ItemCount);

public record PlaceOrderRequest(int AddressId, PaymentMethod PaymentMethod, string? CouponCode, string? CustomerNote);

public record PlaceOrderResult(OrderDto Order, string? PaymentRedirectUrl);

public record UpdateOrderStatusRequest(OrderStatus Status);

public record OrderQueryParams
{
    public OrderStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
