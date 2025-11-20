using Innova.Application.DTOs.Idea;
using Innova.Domain.Enums;

namespace Innova.Application.Validations.Idea
{
    public class CreateIdeaDtoValidator : AbstractValidator<CreateIdeaDto>
    {
        public CreateIdeaDtoValidator()
        {
            Include(new BaseIdeaDtoValidator());

            RuleFor(x => x.IdeaStatus)
                .Equal(IdeaStatus.Draft)
                .WithMessage("New ideas must start in Draft status.");
        }
    }
}