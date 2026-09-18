using FluentValidation;

namespace ECommerce.Application.Coupons;

public class CouponCreateUpdateRequestValidator : AbstractValidator<CouponCreateUpdateRequest>
{
    public CouponCreateUpdateRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiscountType).IsInEnum();
        RuleFor(x => x.DiscountValue).GreaterThan(0);
        RuleFor(x => x.DiscountValue).LessThanOrEqualTo(100)
            .When(x => x.DiscountType == Domain.Enums.DiscountType.Percentage)
            .WithMessage("A percentage discount cannot exceed 100.");
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0).When(x => x.MinOrderAmount.HasValue);
        RuleFor(x => x.MaxUsageCount).GreaterThan(0).When(x => x.MaxUsageCount.HasValue);
    }
}
