using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Orders;

public class OrderService : IOrderService
{
    private const decimal FlatShippingFee = 30_000m;
    private const decimal FreeShippingThreshold = 1_000_000m;

    private readonly IApplicationDbContext _db;
    private readonly IVnPayService _vnPay;
    private readonly IValidator<PlaceOrderRequest> _placeOrderValidator;
    private readonly IValidator<UpdateOrderStatusRequest> _statusValidator;

    public OrderService(
        IApplicationDbContext db,
        IVnPayService vnPay,
        IValidator<PlaceOrderRequest> placeOrderValidator,
        IValidator<UpdateOrderStatusRequest> statusValidator)
    {
        _db = db;
        _vnPay = vnPay;
        _placeOrderValidator = placeOrderValidator;
        _statusValidator = statusValidator;
    }

    public async Task<PlaceOrderResult> PlaceOrderAsync(string userId, PlaceOrderRequest request, string clientIpAddress)
    {
        await _placeOrderValidator.ValidateAndThrowAsync(request);

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId)
            ?? throw new NotFoundException(nameof(Address), request.AddressId);

        var cart = await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null || cart.Items.Count == 0)
            throw new DomainException("Your cart is empty.");

        foreach (var item in cart.Items)
        {
            if (!item.Product.IsActive)
                throw new DomainException($"'{item.Product.Name}' is no longer available.");
            if (item.Quantity > item.Product.StockQuantity)
                throw new DomainException($"Only {item.Product.StockQuantity} unit(s) of '{item.Product.Name}' are available.");
        }

        var subtotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);
        var shippingFee = subtotal >= FreeShippingThreshold ? 0 : FlatShippingFee;
        var discountAmount = 0m;
        string? appliedCouponCode = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode.Trim().ToUpper())
                ?? throw new DomainException("Coupon code is invalid.");

            if (!coupon.IsValidFor(subtotal, DateTime.UtcNow))
                throw new DomainException("Coupon code is expired or not applicable to this order.");

            discountAmount = coupon.CalculateDiscount(subtotal);
            appliedCouponCode = coupon.Code;
            coupon.UsageCount++;
        }

        // Cash-on-delivery orders have no online payment step, so they go straight to processing.
        var initialStatus = request.PaymentMethod == PaymentMethod.CashOnDelivery
            ? OrderStatus.Processing
            : OrderStatus.PendingPayment;

        var order = new Order
        {
            OrderNumber = "PENDING",
            UserId = userId,
            Status = initialStatus,
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            ShippingFee = shippingFee,
            Total = subtotal - discountAmount + shippingFee,
            CouponCode = appliedCouponCode,
            ShippingFullName = address.FullName,
            ShippingPhone = address.Phone,
            ShippingLine1 = address.Line1,
            ShippingLine2 = address.Line2,
            ShippingCity = address.City,
            ShippingProvince = address.Province,
            ShippingPostalCode = address.PostalCode,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            CustomerNote = request.CustomerNote,
            Items = cart.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ProductImageUrl = i.Product.Images.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.DisplayOrder).FirstOrDefault()?.Url,
                UnitPrice = i.Product.Price,
                Quantity = i.Quantity
            }).ToList()
        };

        foreach (var item in cart.Items)
            item.Product.DecreaseStock(item.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        order.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{order.Id:D6}";
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();

        string? redirectUrl = null;
        if (request.PaymentMethod == PaymentMethod.VnPay)
            redirectUrl = _vnPay.CreatePaymentUrl(order, clientIpAddress);

        var dto = await GetOrderAsync(order.Id);
        return new PlaceOrderResult(dto, redirectUrl);
    }

    public async Task<PagedResult<OrderSummaryDto>> GetMyOrdersAsync(string userId, OrderQueryParams query)
    {
        var q = _db.Orders.AsNoTracking().Include(o => o.Items).Where(o => o.UserId == userId);
        return await PageAsync(q, query);
    }

    public async Task<OrderDto> GetMyOrderAsync(string userId, int orderId)
    {
        var order = await _db.Orders.Include(o => o.Items).AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new NotFoundException(nameof(Order), orderId);
        return ToDto(order);
    }

    public async Task<OrderDto> CancelMyOrderAsync(string userId, int orderId)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded)
            throw new DomainException($"An order in '{order.Status}' status can no longer be cancelled.");

        await RestockOrderItemsAsync(order);
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(OrderQueryParams query)
    {
        var q = _db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();
        return await PageAsync(q, query);
    }

    public async Task<OrderDto> GetOrderAsync(int orderId)
    {
        var order = await _db.Orders.Include(o => o.Items).AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);
        return ToDto(order);
    }

    public async Task<OrderDto> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        await _statusValidator.ValidateAndThrowAsync(request);

        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException(nameof(Order), orderId);

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Refunded)
            throw new DomainException($"An order in '{order.Status}' status cannot be changed further.");

        if (request.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            await RestockOrderItemsAsync(order);

        if (request.Status == OrderStatus.Paid)
            order.PaymentStatus = PaymentStatus.Succeeded;

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ToDto(order);
    }

    public async Task<OrderDto> HandlePaymentCallbackAsync(string orderNumber, bool success, string transactionId, string rawResponse)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber)
            ?? throw new NotFoundException(nameof(Order), orderNumber);

        _db.Payments.Add(new Payment
        {
            OrderId = order.Id,
            Provider = order.PaymentMethod,
            ProviderTransactionId = transactionId,
            Amount = order.Total,
            Status = success ? PaymentStatus.Succeeded : PaymentStatus.Failed,
            RawResponse = rawResponse
        });

        if (success)
        {
            order.PaymentStatus = PaymentStatus.Succeeded;
            order.Status = OrderStatus.Paid;
        }
        else
        {
            order.PaymentStatus = PaymentStatus.Failed;
            await RestockOrderItemsAsync(order);
            order.Status = OrderStatus.Cancelled;
        }

        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(order);
    }

    private async Task RestockOrderItemsAsync(Order order)
    {
        foreach (var item in order.Items)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            product?.IncreaseStock(item.Quantity);
        }
    }

    private static async Task<PagedResult<OrderSummaryDto>> PageAsync(IQueryable<Order> q, OrderQueryParams query)
    {
        if (query.Status.HasValue) q = q.Where(o => o.Status == query.Status.Value);
        q = q.OrderByDescending(o => o.CreatedAt);

        var totalCount = await q.CountAsync();
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(o => new OrderSummaryDto(o.Id, o.OrderNumber, o.Status, o.Total, o.PaymentStatus, o.CreatedAt, o.Items.Count))
            .ToListAsync();

        return PagedResult<OrderSummaryDto>.Create(items, query.Page, query.PageSize, totalCount);
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id, o.OrderNumber, o.Status, o.Subtotal, o.DiscountAmount, o.ShippingFee, o.Total, o.CouponCode,
        o.ShippingFullName, o.ShippingPhone, o.ShippingLine1, o.ShippingLine2, o.ShippingCity, o.ShippingProvince, o.ShippingPostalCode,
        o.PaymentMethod, o.PaymentStatus, o.CustomerNote, o.CreatedAt,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.ProductImageUrl, i.UnitPrice, i.Quantity, i.LineTotal)).ToList());
}
