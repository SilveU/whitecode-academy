using Application.Localization;
using Application.DTOs.Authentication;
using Application.Interfaces.Localization;
using Application.Resources;
using FluentValidation;

namespace Application.Validations.Authentecation
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator(IMessageLocalizer<ValidationMessages> localizer)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(50)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength]);

            RuleFor(x => x.LastName)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MaximumLength(50)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength]);

            RuleFor(x => x.UserName)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .MinimumLength(3)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MinLength])
                .MaximumLength(30)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_MaxLength])
                .Matches(@"^[a-zA-Z0-9@._-]+$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidUsername]);

            RuleFor(x => x.Email)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .EmailAddress()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidEmail]);

            RuleFor(x => x.Password)
                .NotEmpty()
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_Required])
                .Matches("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&]{8,}$")
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_InvalidPassword]);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password)
                    .WithMessage(_ => localizer[MessageKeys.Validation.Field_PasswordsMustMatch]);
        }
    }
}
