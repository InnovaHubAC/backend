using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Innova.Application.Validations.Users
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First Name is required")
                .MinimumLength(3)
                .WithMessage("First Name must be at least 3 characters long")
                .MaximumLength(50)
                .WithMessage("First Name must not exceed 50 characters");

            RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last Name is required")
            .MinimumLength(3)
            .WithMessage("Last Name must be at least 3 characters long")
            .MaximumLength(50)
            .WithMessage("Last Name must not exceed 50 characters");
        }
    }
}