namespace Application.DTOs.Authentication
{
    public record AuthResponse
    {
        public bool IsAuthenticated { get; set; } = false;
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}