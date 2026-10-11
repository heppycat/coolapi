using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class UpdateOrderValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status is not valid.");
    }
}
