namespace Application.DTOs.Authentication
{
    public record NewPasswordRequest
    {
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}