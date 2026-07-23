using Application.Localization;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Features.Instructors.Commands.AssignInstructor
{
    public class AssignInstructorValidator : AbstractValidator<AssignInstructorCommand>
    {
        public AssignInstructorValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_UserId_Required]);
        }
    }
}
