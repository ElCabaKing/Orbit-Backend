using FluentValidation;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator()
    {
        When(x => x.DisplayName is not null, () =>
        {
            RuleFor(x => x.DisplayName!)
                .MaximumLength(100).WithMessage("Display name must not exceed 100 characters");
        });

        When(x => x.Bio is not null, () =>
        {
            RuleFor(x => x.Bio!)
                .MaximumLength(500).WithMessage("Bio must not exceed 500 characters");
        });
    }
}
