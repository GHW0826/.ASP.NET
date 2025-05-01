using JWTToken.Models.Dtos;
using FluentValidation;

namespace JWTToken.Validators
{
    public class TodoUpdateDtoValidator : AbstractValidator<UpdateTodoDto>
    {
        public TodoUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title은 필수입니다.")
                .MaximumLength(100).WithMessage("Title은 100자 이하여야 합니다.");

            RuleFor(x => x.IsComplete)
                .NotNull().WithMessage("IsComplete는 필수입니다.");
        }
    }
}
