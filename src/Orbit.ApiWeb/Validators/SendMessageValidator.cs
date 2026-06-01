using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;
using Orbit.Shared.Constants;

namespace Orbit.ApiWeb.Validators;

public class SendMessageValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ValidationConstants.ContentRequired)
            .MaximumLength(DomainConstants.MessageContentMaxLength).WithMessage(ValidationConstants.ContentMaxLengthMessage);
    }
}
