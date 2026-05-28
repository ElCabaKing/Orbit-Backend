using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class CreateCommentValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ValidationConstants.ContentRequired)
            .MaximumLength(500).WithMessage(ValidationConstants.ContentMaxLengthComment);
    }
}
