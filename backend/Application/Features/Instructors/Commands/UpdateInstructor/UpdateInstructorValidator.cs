using Application.Localization;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Features.Instructors.Commands.UpdateInstructor
{
    public class UpdateInstructorValidator : AbstractValidator<UpdateInstructorCommand>
    {
        public UpdateInstructorValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_DepartmentId_Invalid])
                .When(x => x.DepartmentId.HasValue);
        }
    }
}
