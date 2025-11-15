using Innova.Application.DTOs.Idea;

namespace Innova.Application.Validations.Idea
{
    public class BaseIdeaDtoValidator : AbstractValidator<BaseIdeaDto>
    {
        public BaseIdeaDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .MaximumLength(250)
                .WithMessage("Title must not exceed 250 characters");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Content is required")
                .MaximumLength(2500)
                .WithMessage("Content must not exceed 2500 characters");

            RuleFor(x => x.AppUserId)
                .NotEmpty()
                .WithMessage("AppUserId is required");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("DepartmentId must be greater than 0");
        }
    }
}
