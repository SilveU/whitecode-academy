using Domain.Entites.Core;
using Domain.Entites.Enums;
using Domain.Entites.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data.Seeding
{
    public static class AppSeeder
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager, configuration);
            await SeedDepartmentsAsync(context);
            await SeedInstructorsAsync(context, userManager, configuration);
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

        // 2. Users — credentials come from IConfiguration (user-secrets in dev, env vars in prod)
        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            var admin      = configuration.GetSection("SeedSettings:Admin");
            var instructor = configuration.GetSection("SeedSettings:Instructor");
            var student    = configuration.GetSection("SeedSettings:Student");

            await CreateUserIfNotExists(userManager,
                email:     admin["Email"]     ?? throw new InvalidOperationException("SeedSettings:Admin:Email is not configured."),
                username:  admin["UserName"]  ?? throw new InvalidOperationException("SeedSettings:Admin:UserName is not configured."),
                firstName: admin["FirstName"] ?? "System",
                lastName:  admin["LastName"]  ?? "Admin",
                password:  admin["Password"]  ?? throw new InvalidOperationException("SeedSettings:Admin:Password is not configured."),
                role:      "Admin");

            await CreateUserIfNotExists(userManager,
                email:     instructor["Email"]     ?? throw new InvalidOperationException("SeedSettings:Instructor:Email is not configured."),
                username:  instructor["UserName"]  ?? throw new InvalidOperationException("SeedSettings:Instructor:UserName is not configured."),
                firstName: instructor["FirstName"] ?? "Instructor",
                lastName:  instructor["LastName"]  ?? "Instructor",
                password:  instructor["Password"]  ?? throw new InvalidOperationException("SeedSettings:Instructor:Password is not configured."),
                role:      "Instructor");

            await CreateUserIfNotExists(userManager,
                email:     student["Email"]     ?? throw new InvalidOperationException("SeedSettings:Student:Email is not configured."),
                username:  student["UserName"]  ?? throw new InvalidOperationException("SeedSettings:Student:UserName is not configured."),
                firstName: student["FirstName"] ?? "Student",
                lastName:  student["LastName"]  ?? "Student",
                password:  student["Password"]  ?? throw new InvalidOperationException("SeedSettings:Student:Password is not configured."),
                role:      "User");
        }

        private static async Task<ApplicationUser> CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email, string username, string firstName, string lastName,
            string password, string role)
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

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create seed user '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        // 3. Departments — static data, safe to keep here
        public static async Task SeedDepartmentsAsync(ApplicationDbContext context)
        {
            if (context.Departments.Any()) return;

            var departments = new List<Department>
            {
                new() { Id = Guid.NewGuid(), Name = "Computer Science", Description = "The study of computers and computational systems.",     CreatedAt = DateTimeOffset.UtcNow.AddDays(-10) },
                new() { Id = Guid.NewGuid(), Name = "Mathematics",       Description = "The study of numbers, quantities, and shapes.",          CreatedAt = DateTimeOffset.UtcNow.AddDays(-30) },
                new() { Id = Guid.NewGuid(), Name = "Physics",           Description = "The study of matter, energy, and their interactions.",   CreatedAt = DateTimeOffset.UtcNow.AddDays(-90) },
                new() { Id = Guid.NewGuid(), Name = "Chemistry",         Description = "The study of substances and their transformations.",     CreatedAt = DateTimeOffset.UtcNow.AddDays(-6)  },
                new() { Id = Guid.NewGuid(), Name = "Biology",           Description = "The study of living organisms.",                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-20) }
            };

            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();
        }

        // 4. Instructors
        public static async Task SeedInstructorsAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            if (context.Instructors.Any()) return;

            var instructorEmail = configuration["SeedSettings:Instructor:Email"]
                ?? throw new InvalidOperationException("SeedSettings:Instructor:Email is not configured.");

            var instructorUser = await userManager.FindByEmailAsync(instructorEmail);
            if (instructorUser == null)
                throw new InvalidOperationException(
                    $"Instructor user '{instructorEmail}' not found. Ensure users are seeded before instructors.");

            var csDepartment = context.Departments.FirstOrDefault(x => x.Name == "Computer Science");
            if (csDepartment == null)
                throw new InvalidOperationException("Computer Science department not found. Ensure departments are seeded first.");

            context.Instructors.Add(new Instructor
            {
                Id           = Guid.NewGuid(),
                UserId       = instructorUser.Id,
                DepartmentId = csDepartment.Id,
                CreatedAt    = DateTimeOffset.UtcNow.AddDays(-15)
            });

            await context.SaveChangesAsync();
        }
    }
}
