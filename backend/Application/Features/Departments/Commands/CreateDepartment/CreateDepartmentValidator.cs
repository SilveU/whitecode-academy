using Application.Localization;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        public CreateDepartmentValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(100)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_DepartmentName_MaxLength])
                .Matches(@"^[a-zA-Z0-9\s]+$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidNameFormat]);

            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(500)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_DepartmentDescription_MaxLength]);

            // ImageFile is optional — validate only when provided
            When(x => x.ImageFile != null, () =>
            {
                RuleFor(x => x.ImageFile!)
                    .Must(f => AllowedImageExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                        .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidImageExtension])
                    .Must(f => f.Length <= MaxImageSizeBytes)
                        .WithMessage(_ => localizer[MessageKeys.Validation.Field_ImageSizeExceeded]);
            });
        }
    }
}
