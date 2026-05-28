using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.EmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.EmailInvalidFormat);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(ValidationConstants.TokenRequired)
            .Length(6).WithMessage(ValidationConstants.TokenLength);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(ValidationConstants.NewPasswordRequired)
            .MinimumLength(8).WithMessage(ValidationConstants.PasswordMinLength)
            .Matches("[A-Z]").WithMessage(ValidationConstants.PasswordUppercase)
            .Matches("[0-9]").WithMessage(ValidationConstants.PasswordNumber);
    }
}
