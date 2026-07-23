using Application.Localization;
using Application.DTOs.Authentication;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Validations.Authentecation
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.Identity)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(100)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength]);

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required]);
        }
    }
}
