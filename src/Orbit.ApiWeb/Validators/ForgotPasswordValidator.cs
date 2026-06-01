using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage(ValidationConstants.EmailOrUsernameRequired)
            .MaximumLength(255).WithMessage(ValidationConstants.EmailMaxLength);
    }
}
