using FluentValidation;

namespace Application.Features.Departments.Commands.UpdateDepartment
{
    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        public UpdateDepartmentValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Name can only contain letters, numbers, and spaces.")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // ImageFile is optional — validate only when provided
            When(x => x.ImageFile != null, () =>
            {
                RuleFor(x => x.ImageFile!)
                    .Must(f => AllowedImageExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                    .WithMessage($"Image must be one of: {string.Join(", ", AllowedImageExtensions)}.")
                    .Must(f => f.Length <= MaxImageSizeBytes)
                    .WithMessage("Image size cannot exceed 5 MB.");
            });
        }
    }
}
