namespace Application.DTOs.Authentication
{
    public record EmailResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}