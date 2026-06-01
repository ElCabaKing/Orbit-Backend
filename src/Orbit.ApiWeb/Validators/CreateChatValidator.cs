using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class CreateChatValidator : AbstractValidator<CreateChatRequest>
{
    public CreateChatValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ValidationConstants.UsernameRequired)
            .Length(3, 30).WithMessage(ValidationConstants.UsernameLength)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage(ValidationConstants.UsernameInvalidChars);
    }
}
