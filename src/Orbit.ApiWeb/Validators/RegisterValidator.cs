using FluentValidation;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 30).WithMessage("Username must be between 3 and 30 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required")
            .MaximumLength(100).WithMessage("Display name must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters");

        When(x => x.ProfilePicture is not null, () =>
        {
            RuleFor(x => x.ProfilePicture!)
                .Must(BeValidExtension).WithMessage("Profile picture must be jpg, jpeg, png or webp")
                .Must(f => f.Length <= MaxFileSize).WithMessage("Profile picture must not exceed 5MB");
        });
    }

    private static bool BeValidExtension(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }
}
