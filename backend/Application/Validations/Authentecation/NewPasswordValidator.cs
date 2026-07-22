using Application.DTOs.Authentication;
using FluentValidation;

namespace Application.Validations.Authentecation
{
    public class NewPasswordValidator : AbstractValidator<NewPasswordRequest>
    {
        public NewPasswordValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Password is required.")
                .Matches("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&]{8,}$")
                .WithMessage("Password must contain letter, number, and special character.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}