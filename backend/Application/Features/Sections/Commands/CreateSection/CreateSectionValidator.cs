using Application.Localization;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Features.Sections.Commands.CreateSection
{
    public class CreateSectionValidator : AbstractValidator<CreateSectionCommand>
    {
        public CreateSectionValidator(IMessageLocalizer<ValidationMessages> localizer) // عشان يتعمل localization في نفس اللحظه 
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(200)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_SectionName_MaxLength]);

            RuleFor(x => x.Description)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(1000)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_SectionDescription_MaxLength]);

            RuleFor(x => x.CourseId)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_CourseId_Required]);
        }
    }
}
