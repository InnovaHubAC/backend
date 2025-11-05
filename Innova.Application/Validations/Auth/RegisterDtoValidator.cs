using Innova.Application.DTOs.Auth;

namespace Innova.Application.Validations.Auth
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .MinimumLength(3)
                .WithMessage("First name must be at least 3 characters long")
                .When(x => !string.IsNullOrEmpty(x.FirstName))
                .MaximumLength(100)
                .WithMessage("First name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
               .MinimumLength(3)
                .WithMessage("Last name must be at least 3 characters long")
                .When(x => !string.IsNullOrEmpty(x.LastName))
                .MaximumLength(100)
                .WithMessage("Last name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .MaximumLength(128)
                .WithMessage("Email must not exceed 128 characters")
                .EmailAddress()
                .WithMessage("The email must be valid");

            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required")
                .MaximumLength(50)
                .WithMessage("Username must not exceed 50 characters");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MaximumLength(256)
                .WithMessage("Password must not exceed 256 characters")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long")
                .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"\d")
                .WithMessage("Password must contain at least one number")
                .Matches(@"[#@$!%*?&]")
                .WithMessage("Password must contain at least one special character (@$!%*?&#)");
        }
    }
}
