namespace Application.DTOs.Authentication
{
    public record LoginRequest
    {
        public string Identity { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}