using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email address is not valid.")
            .MaximumLength(254)
            .WithMessage("Email must not exceed 254 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(128)
            .WithMessage("Password must not exceed 128 characters.");
    }
}
