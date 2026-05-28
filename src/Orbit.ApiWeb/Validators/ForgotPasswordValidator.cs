using FluentValidation;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
