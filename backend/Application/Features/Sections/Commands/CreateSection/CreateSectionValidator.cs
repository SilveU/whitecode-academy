using FluentValidation;

namespace Application.Features.Sections.Commands.CreateSection
{
    public class CreateSectionValidator : AbstractValidator<CreateSectionCommand>
    {
        public CreateSectionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("CourseId is required.");

            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt).WithMessage("End time must be after start time.");
        }
    }
}
