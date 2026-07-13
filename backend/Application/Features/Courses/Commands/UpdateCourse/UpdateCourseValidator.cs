using FluentValidation;

namespace Application.Features.Courses.Commands.UpdateCourse
{
    public class UpdateCourseValidator : AbstractValidator<UpdateCourseCommand>
    {
        public UpdateCourseValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200)
                .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Name can only contain letters, numbers, and spaces.")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Description can only contain letters, numbers, and spaces.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}