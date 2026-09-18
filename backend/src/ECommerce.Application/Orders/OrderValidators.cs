using FluentValidation;

namespace ECommerce.Application.Orders;

public class PlaceOrderRequestValidator : AbstractValidator<PlaceOrderRequest>
{
    public PlaceOrderRequestValidator()
    {
        RuleFor(x => x.AddressId).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.CouponCode).MaximumLength(50);
    }
}

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
