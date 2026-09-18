using FluentValidation;

namespace ECommerce.Application.Categories;

public class CategoryCreateUpdateRequestValidator : AbstractValidator<CategoryCreateUpdateRequest>
{
    public CategoryCreateUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
