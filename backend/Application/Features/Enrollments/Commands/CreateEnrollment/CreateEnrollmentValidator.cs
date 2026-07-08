using FluentValidation;

namespace Application.Features.Enrollments.Commands.CreateEnrollment
{
    public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentCommand>
    {
        public CreateEnrollmentValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("CourseId is required.");
        }
    }
}
