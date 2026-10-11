using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("ProductId is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}
