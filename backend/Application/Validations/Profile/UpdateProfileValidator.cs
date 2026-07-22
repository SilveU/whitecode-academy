using Application.DTOs.Profile;
using FluentValidation;

namespace Application.Validations.Profile
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequset>
    {
        private static readonly string[] AllowedPdfExtensions   = { ".jpg", ".jpeg", ".png" };
        private const long MaxPdfSizeBytes   = 5 * 1024 * 1024;     // 5 MB
        public UpdateProfileValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.UserName))
                .WithMessage("First name is required.")
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.UserName))
                .WithMessage("Last name is required.")
                .MaximumLength(50);

            RuleFor(x => x.UserName)
                .NotEmpty()
                .When(x => !string.IsNullOrEmpty(x.UserName))
                .MinimumLength(3)
                .MaximumLength(30)
                .Matches(@"^[a-zA-Z0-9@._-]+$")
                .WithMessage("Username can only contain letters, numbers, ., _, -, @");

            // ImageUrl is optional — validate only when provided
            When(x => x.ImageUrl != null, () =>
            {
                RuleFor(x => x.ImageUrl!)
                    .Must(f => AllowedPdfExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                    .WithMessage("Image must be a .jpg, .jpeg, .png file.")
                    .Must(f => f.Length <= MaxPdfSizeBytes)
                    .WithMessage("PDF size cannot exceed 5 MB.");
            });
        }
    }
}