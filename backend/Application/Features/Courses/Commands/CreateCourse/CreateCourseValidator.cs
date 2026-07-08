using FluentValidation;

namespace Application.Features.Courses.Commands.CreateCourse
{
    public class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
    {
        public CreateCourseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200)
                .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Name can only contain letters, numbers, and spaces.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(1000)
                .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Description can only contain letters, numbers, and spaces.");

            RuleFor(x => x.TotalHours)
                .GreaterThan(0)
                .LessThanOrEqualTo(500);

            RuleFor(x => x.TotalSections)
                .GreaterThan(0)
                .LessThanOrEqualTo(250);
        }
    }
}