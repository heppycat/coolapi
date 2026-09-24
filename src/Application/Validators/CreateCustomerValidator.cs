using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MinimumLength(3)
            .WithMessage("Name is too short.")
            .MaximumLength(20)
            .WithMessage("Name is too long.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email address is not valid.")
            .MaximumLength(254)
            .WithMessage("Email must not exceed 254 characters.");
    }
}
