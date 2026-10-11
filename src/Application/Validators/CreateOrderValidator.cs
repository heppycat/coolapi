using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("CustomerId is required.");

        RuleFor(x => x.Items).NotEmpty().WithMessage("An order needs at least one item.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
    }
}
