using Domain.Entites.Enums;
using Domain.Entites.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data.Seeding
{
    public static class AppSeeder
    {
         public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
        }

        // 1. Roles
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Enum.GetValues<Role>())
            {
                var roleName = role.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Users
        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            await CreateUserIfNotExists(userManager, "admin@system.com", "admin@system", "System", "Admin", "Admin");
            await CreateUserIfNotExists(userManager, "instructor@system.com", "instructor@system", "Instructor", "Instructor", "Instructor");
            await CreateUserIfNotExists(userManager, "student@system.com", "student@system", "Student", "Student", "Student");
        }

        private static async Task<ApplicationUser> CreateUserIfNotExists(UserManager<ApplicationUser> userManager,
        string email, string username, string firstName, string lastName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null) return user;

            user = new ApplicationUser
            {
                UserName       = username,
                Email          = email,
                FirstName      = firstName,
                LastName       = lastName,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, $"{role}@123");
            await userManager.AddToRoleAsync(user, role);
            return user;
        }
    }
}