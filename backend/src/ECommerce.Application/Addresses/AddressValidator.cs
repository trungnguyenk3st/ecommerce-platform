using FluentValidation;

namespace ECommerce.Application.Addresses;

public class AddressCreateUpdateRequestValidator : AbstractValidator<AddressCreateUpdateRequest>
{
    public AddressCreateUpdateRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(80);
    }
}
