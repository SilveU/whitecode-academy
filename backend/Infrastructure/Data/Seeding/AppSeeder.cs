using Domain.Entites.Core;
using Domain.Entites.Enums;
using Domain.Entites.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data.Seeding
{
    public static class AppSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedDepartmentsAsync(context);
            await SeedInstructorsAsync(context, userManager);
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

        // 3. Departments
        public static async Task SeedDepartmentsAsync(ApplicationDbContext context)
        {
            if (!context.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new Department 
                    { 
                        Id = Guid.NewGuid(),
                        Name = "Computer Science",
                        Description = "The study of computers and computational systems.",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-10), // Set a past date for CreatedAt
                        UpdatedAt = null,
                        DeletedAt = null,
                        IsDeleted = false

                    },
                    new Department 
                    { 
                        Id = Guid.NewGuid(), 
                        Name = "Mathematics", 
                        Description = "The study of numbers, quantities, and shapes." ,
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-30), // Set a past date for CreatedAt
                        UpdatedAt = null,
                        DeletedAt = null,
                        IsDeleted = false
                    },
                    new Department 
                    { 
                        Id = Guid.NewGuid(), 
                        Name = "Physics", 
                        Description = "The study of matter, energy, and their interactions." ,
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-90), // Set a past date for CreatedAt
                        UpdatedAt = null,
                        DeletedAt = null,
                        IsDeleted = false
                    },
                    new Department 
                    { 
                        Id = Guid.NewGuid(), 
                        Name = "Chemistry", 
                        Description = "The study of substances and their transformations." ,
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-6), // Set a past date for CreatedAt
                        UpdatedAt = null,
                        DeletedAt = null,
                        IsDeleted = false
                    },
                    new Department 
                    { 
                        Id = Guid.NewGuid(), 
                        Name = "Biology", 
                        Description = "The study of living organisms." ,
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-20), // Set a past date for CreatedAt
                        UpdatedAt = null,
                        DeletedAt = null,
                        IsDeleted = false
                    }
                };
                context.Departments.AddRange(departments);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedInstructorsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!context.Instructors.Any())
            {
                var instructorUser = await userManager.FindByEmailAsync("instructor@system.com");
                if (instructorUser == null)
                {
                    throw new 
                    Exception("Instructor user not found. Please ensure the instructor user is seeded before seeding instructors.");
                }

                var instructors = new List<Instructor>
                {
                    new Instructor
                    {
                        Id = Guid.NewGuid(),
                        UserId = instructorUser.Id,
                        DepartmentId = context.Departments.First(x => x.Name == "Computer Science").Id,
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-15), // Set a past date for CreatedAt
                        UpdatedAt = null,
                        DeletedAt = null,
                        IsDeleted = false
                    }
                };

                context.Instructors.AddRange(instructors);
                await context.SaveChangesAsync();
            }
        }
    }
}