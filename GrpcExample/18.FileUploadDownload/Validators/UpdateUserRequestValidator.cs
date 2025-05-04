using FluentValidation;
using FileUploadDownload;

namespace FileUploadDownload.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(30).WithMessage("Name must be under 30 characters.");

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 120).WithMessage("Age must be between 1 and 120.");
    }
}
