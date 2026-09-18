using FluentValidation;

namespace ECommerce.Application.Products;

public class ProductCreateUpdateRequestValidator : AbstractValidator<ProductCreateUpdateRequest>
{
    public ProductCreateUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CompareAtPrice).GreaterThan(0).When(x => x.CompareAtPrice.HasValue);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleForEach(x => x.Images).ChildRules(image =>
        {
            image.RuleFor(i => i.Url).NotEmpty();
        });
    }
}

public class ProductQueryParamsValidator : AbstractValidator<ProductQueryParams>
{
    private static readonly string[] AllowedSorts = { "price_asc", "price_desc", "newest", "rating", "name" };

    public ProductQueryParamsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortBy).Must(s => s is null || AllowedSorts.Contains(s))
            .WithMessage($"sortBy must be one of: {string.Join(", ", AllowedSorts)}");
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(x => x.MinPrice!.Value)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);
    }
}

public class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.DeltaQuantity).NotEqual(0);
    }
}
