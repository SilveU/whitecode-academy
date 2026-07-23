using Application.Localization;
using Application.DTOs.Authentication;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Validations.Authentecation
{
    public class ResendEmailConfirmationValidator : AbstractValidator<ResendEmailConfirmationRequest>
    {
        public ResendEmailConfirmationValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .EmailAddress()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidEmail]);
        }
    }
}
