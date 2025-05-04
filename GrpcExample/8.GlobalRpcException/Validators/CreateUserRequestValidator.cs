using FluentValidation;
using GlobalRpcException;

namespace GlobalRpcException.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(30).WithMessage("Name must be under 30 characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 120).WithMessage("Age must be between 1 and 120.");
    }
}
