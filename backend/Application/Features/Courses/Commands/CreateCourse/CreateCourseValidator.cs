using Application.Localization;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Features.Courses.Commands.CreateCourse
{
    public class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
    {
        public CreateCourseValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(200)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength])
                .Matches(@"^[a-zA-Z0-9\s]+$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidNameFormat]);

            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(1000)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength])
                .Matches(@"^[a-zA-Z0-9\s]+$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidDescriptionFormat]);
        }
    }
}
