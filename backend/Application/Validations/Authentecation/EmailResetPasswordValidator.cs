using Application.DTOs.Authentication;
using FluentValidation;

namespace Application.Validations.Authentecation
{
    public class EmailResetPasswordValidator : AbstractValidator<EmailResetPasswordRequest>
    {
        public EmailResetPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}