using Innova.Application.Validations.Idea;

namespace Innova.Application.DTOs.Idea
{
    public class UpdateIdeaDtoValidator : AbstractValidator<UpdateIdeaDto>
    {
        public UpdateIdeaDtoValidator()
        {
            Include(new BaseIdeaDtoValidator());

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than 0");
        }
    }
}