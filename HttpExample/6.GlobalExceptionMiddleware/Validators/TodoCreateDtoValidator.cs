using _6.GlobalExceptionMiddleware.Models.Dtos;
using FluentValidation;

namespace _6.GlobalExceptionMiddleware.Validators
{
    public class TodoCreateDtoValidator : AbstractValidator<CreateTodoDto>
    {
        public TodoCreateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title은 필수입니다.")
                .MaximumLength(100).WithMessage("Title은 100자 이하여야 합니다.");
        }
    }
}
