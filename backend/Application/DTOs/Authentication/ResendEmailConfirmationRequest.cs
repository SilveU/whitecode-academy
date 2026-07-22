namespace Application.DTOs.Authentication
{
    public record ResendEmailConfirmationRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}