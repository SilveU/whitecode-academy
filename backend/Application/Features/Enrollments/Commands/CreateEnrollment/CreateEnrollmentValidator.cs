using Application.Localization;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Features.Enrollments.Commands.CreateEnrollment
{
    public class CreateEnrollmentValidator : AbstractValidator<CreateEnrollmentCommand>
    {
        public CreateEnrollmentValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.CourseId)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_CourseId_Required]);
        }
    }
}
