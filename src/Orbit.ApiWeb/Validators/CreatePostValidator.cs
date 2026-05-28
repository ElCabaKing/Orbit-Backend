using FluentValidation;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class CreatePostValidator : AbstractValidator<CreatePostRequest>
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov", ".avi", ".webm"];
    private const long MaxFileSize = 10 * 1024 * 1024;

    public CreatePostValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(1000).WithMessage("Content must not exceed 1000 characters");

        When(x => x.Media is not null && x.Media.Count > 0, () =>
        {
            RuleForEach(x => x.Media!)
                .Must(BeValidExtension).WithMessage("Each file must be jpg, jpeg, png, webp, gif, mp4, mov, avi or webm")
                .Must(f => f.Length <= MaxFileSize).WithMessage("Each file must not exceed 10MB");

            RuleFor(x => x.Media!)
                .Must(media => media.Count <= 10)
                .WithMessage("Maximum 10 files allowed per post");
        });
    }

    private static bool BeValidExtension(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }
}
