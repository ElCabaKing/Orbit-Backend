using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.EmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.EmailInvalidFormat);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationConstants.PasswordRequired);
    }
}
