using FluentValidation;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class CreateCommentValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(500).WithMessage("Content must not exceed 500 characters");
    }
}
