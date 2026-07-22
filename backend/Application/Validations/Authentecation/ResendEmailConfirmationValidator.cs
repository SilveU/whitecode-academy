using Application.DTOs.Authentication;
using FluentValidation;

namespace Application.Validations.Authentecation
{
    public class ResendEmailConfirmationValidator : AbstractValidator<ResendEmailConfirmationRequest>
    {
        public ResendEmailConfirmationValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}