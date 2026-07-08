using FluentValidation;

namespace Application.Features.Sections.Commands.UpdateSection
{
    public class UpdateSectionValidator : AbstractValidator<UpdateSectionCommand>
    {
        public UpdateSectionValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.VideoUrl)
                .MaximumLength(500).WithMessage("Video URL cannot exceed 500 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Video URL must be a valid URL.")
                .When(x => !string.IsNullOrEmpty(x.VideoUrl));

            RuleFor(x => x.PdfUrl)
                .MaximumLength(500).WithMessage("PDF URL cannot exceed 500 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("PDF URL must be a valid URL.")
                .When(x => !string.IsNullOrEmpty(x.PdfUrl));

            RuleFor(x => x)
                .Must(x => x.EndAt > x.StartAt).WithMessage("End time must be after start time.")
                .When(x => x.StartAt.HasValue && x.EndAt.HasValue);
        }
    }
}
