using FluentValidation;
using Orbit.ApiWeb.Constants;
using Orbit.ApiWeb.DTOs;

namespace Orbit.ApiWeb.Validators;

public class CreatePostValidator : AbstractValidator<CreatePostRequest>
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".mp4", ".mov", ".avi", ".webm"];
    private const long MaxFileSize = 10 * 1024 * 1024;

    public CreatePostValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ValidationConstants.ContentRequired)
            .MaximumLength(1000).WithMessage(ValidationConstants.ContentMaxLength);

        When(x => x.Media is not null && x.Media.Count > 0, () =>
        {
            RuleForEach(x => x.Media!)
                .Must(BeValidExtension).WithMessage(ValidationConstants.MediaInvalidExtension)
                .Must(f => f.Length <= MaxFileSize).WithMessage(ValidationConstants.MediaMaxSize);

            RuleFor(x => x.Media!)
                .Must(media => media.Count <= 10)
                .WithMessage(ValidationConstants.MediaMaxCount);
        });
    }

    private static bool BeValidExtension(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }
}
