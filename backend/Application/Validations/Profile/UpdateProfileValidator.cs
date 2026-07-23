using Application.Localization;
using Application.DTOs.Profile;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Validations.Profile
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequset>
    {
        private static readonly string[] AllowedPdfExtensions   = { ".jpg", ".jpeg", ".png" };
        private const long MaxPdfSizeBytes   = 5 * 1024 * 1024;     // 5 MB
        
        public UpdateProfileValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .When(x => !string.IsNullOrEmpty(x.UserName))
                .MaximumLength(50)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength]);

            RuleFor(x => x.LastName)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .When(x => !string.IsNullOrEmpty(x.UserName))
                .MaximumLength(50)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength]);

            RuleFor(x => x.UserName)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .When(x => !string.IsNullOrEmpty(x.UserName))
                .MinimumLength(3)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MinLength])
                .MaximumLength(30)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength])
                .Matches(@"^[a-zA-Z0-9@._-]+$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidUsername]);

            // ImageUrl is optional — validate only when provided
            When(x => x.ImageUrl != null, () =>
            {
                RuleFor(x => x.ImageUrl!)
                    .Must(f => AllowedPdfExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                        .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidImageFileType])
                    .Must(f => f.Length <= MaxPdfSizeBytes)
                        .WithMessage(_ => localizer[MessageKeys.Validation.Field_ProfileImageSizeExceeded]);
            });
        }
    }
}
