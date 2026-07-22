using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Profile
{
    public record UpdateProfileRequset
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}