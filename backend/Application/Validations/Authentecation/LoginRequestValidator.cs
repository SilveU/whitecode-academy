using Application.DTOs.Authentication;
using FluentValidation;

namespace Application.Validations.Authentecation
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Identity)
                .NotEmpty().WithMessage("Identity is required.")
                .MaximumLength(100);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}