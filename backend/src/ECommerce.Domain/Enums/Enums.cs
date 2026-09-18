namespace ECommerce.Domain.Enums;

public enum OrderStatus
{
    PendingPayment = 0,
    Paid = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6
}

public enum PaymentMethod
{
    CashOnDelivery = 0,
    VnPay = 1,
    Momo = 2
}

public enum PaymentStatus
{
    Pending = 0,
    Succeeded = 1,
    Failed = 2,
    Refunded = 3
}

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1
}
