using FluentValidation;

namespace Application.Features.Sections.Commands.UpdateSection
{
    public class UpdateSectionValidator : AbstractValidator<UpdateSectionCommand>
    {
        private static readonly string[] AllowedPdfExtensions   = { ".pdf" };
        private const long MaxVideoSizeBytes = 1000L * 1024 * 1024; // 1000 MB
        private const long MaxPdfSizeBytes   = 5 * 1024 * 1024;     // 5 MB

        public UpdateSectionValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            // PdfFile is optional — validate only when provided
            When(x => x.PdfFile != null, () =>
            {
                RuleFor(x => x.PdfFile!)
                    .Must(f => AllowedPdfExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
                    .WithMessage("PDF must be a .pdf file.")
                    .Must(f => f.Length <= MaxPdfSizeBytes)
                    .WithMessage("PDF size cannot exceed 5 MB.");
            });
        }
    }
}
