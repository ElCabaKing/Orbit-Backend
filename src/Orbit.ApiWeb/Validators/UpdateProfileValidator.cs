using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator()
    {
        When(x => x.DisplayName is not null, () =>
        {
            RuleFor(x => x.DisplayName!)
                .MaximumLength(100).WithMessage(ValidationConstants.DisplayNameMaxLength);
        });

        When(x => x.Bio is not null, () =>
        {
            RuleFor(x => x.Bio!)
                .MaximumLength(500).WithMessage(ValidationConstants.BioMaxLength);
        });
    }
}
