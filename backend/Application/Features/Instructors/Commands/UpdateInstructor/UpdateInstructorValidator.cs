using FluentValidation;

namespace Application.Features.Instructors.Commands.UpdateInstructor
{
    public class UpdateInstructorValidator : AbstractValidator<UpdateInstructorCommand>
    {
        public UpdateInstructorValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("DepartmentId cannot be an empty GUID.")
                .When(x => x.DepartmentId.HasValue);
        }
    }
}
