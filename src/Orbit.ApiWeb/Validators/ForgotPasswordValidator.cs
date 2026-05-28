using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.EmailRequired)
            .MaximumLength(255).WithMessage(ValidationConstants.EmailMaxLength)
            .EmailAddress().WithMessage(ValidationConstants.EmailInvalidFormat);
    }
}
