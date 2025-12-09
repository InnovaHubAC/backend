using FluentValidation;
using Innova.Application.DTOs.Messaging;

namespace Innova.Application.Validations.Messaging;

public class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageDtoValidator()
    {
        RuleFor(x => x.ReceiverId)
            .NotEmpty().WithMessage("Receiver ID is required")
            .MaximumLength(450).WithMessage("Receiver ID cannot exceed 450 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(4000).WithMessage("Message content cannot exceed 4000 characters");
    }
}
