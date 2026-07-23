using Application.Localization;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Features.Courses.Commands.UpdateCourse
{
    public class UpdateCourseValidator : AbstractValidator<UpdateCourseCommand>
    {
        public UpdateCourseValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.Name)
                .MaximumLength(200)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength])
                .Matches(@"^[a-zA-Z0-9\s]+$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidNameFormat])
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength])
                .Matches(@"^[a-zA-Z0-9\s]+$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidDescriptionFormat])
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
