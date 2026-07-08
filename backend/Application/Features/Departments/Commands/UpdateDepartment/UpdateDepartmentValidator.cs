using FluentValidation;

namespace Application.Features.Departments.Commands.UpdateDepartment
{
    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9\s]+$").WithMessage("Name can only contain letters, numbers, and spaces.")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("Image URL must be a valid URL.")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));
        }
    }
}
