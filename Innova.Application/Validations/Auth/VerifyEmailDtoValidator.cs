using FluentValidation;
using Innova.Application.DTOs.Auth;

namespace Innova.Application.Validations.Auth
{
    public class VerifyEmailDtoValidator : AbstractValidator<VerifyEmailDto>
    {
        public VerifyEmailDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Verification token is required.");
        }
    }
}
