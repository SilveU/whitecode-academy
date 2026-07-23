using Application.Localization;
using Application.DTOs.Authentication;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Validations.Authentecation
{
    public class NewPasswordValidator : AbstractValidator<NewPasswordRequest>
    {
        public NewPasswordValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .Matches("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&]{8,}$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidPassword]);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_PasswordsMustMatch]);
        }
    }
}
