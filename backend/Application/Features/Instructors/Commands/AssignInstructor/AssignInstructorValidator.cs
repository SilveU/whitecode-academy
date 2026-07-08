using FluentValidation;

namespace Application.Features.Instructors.Commands.AssignInstructor
{
    public class AssignInstructorValidator : AbstractValidator<AssignInstructorCommand>
    {
        public AssignInstructorValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");
        }
    }
}
