using Microsoft.AspNetCore.Identity;

namespace Domain.Entites.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string ProviderId { get; set; } = Guid.CreateVersion7().ToString();
        public string Provider { get; set; } = "Local";
    }
}
