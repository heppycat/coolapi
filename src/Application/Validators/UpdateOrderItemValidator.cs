using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class UpdateOrderItemValidator : AbstractValidator<UpdateOrderItemDto>
{
    public UpdateOrderItemValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}
