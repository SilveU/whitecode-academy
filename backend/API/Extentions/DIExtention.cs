using API.Mapping;
using Application.Features.Courses.Commands.CreateCourse;
using Application.Interfaces.Authentecation;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Validations;
using Domain.Entites.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Authentecation;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Extentions
{
    public static class DIExtention
    {
        public static void AddDependencyInjectionExtention(this IServiceCollection service, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 2. Register your DbContext in the DI container
            service.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 3. Register Identity services
            service
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // 5. Register Authentication services
            service.AddScoped<IAuthenticationService, AuthenticationService>();
            service.AddScoped<IEmailVerificationService, EmailVerificationService>();
            service.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // 6. Register AutoMapper
            service.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            // 7. Register FluentValidation
            service.AddFluentValidationAutoValidation();
            service.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

            // 8. Register other services
            service.AddScoped<IEmailSender, EmailSender>();
            service.AddScoped<IFileStorageService, LocalFileStorageService>();
            service.AddScoped<IFileSecurityService, ClamAvFileScanner>();

            // 9. Register MediatR and scan the assembly where the Program class lives
            service.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(typeof(CreateCourseHandler).Assembly));

            // 10. Register repositories
            service.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            service.AddScoped<ICourseRepository, CourseRepository>();
            service.AddScoped<IInstructorRepository, InstructorRepository>();
            service.AddScoped<IDepartmentRepository, DepartmentRepository>();
            service.AddScoped<ISectionRepository, SectionRepository>();
            service.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            service.AddScoped<IStudentRepository, StudentRepository>();
        }
    }
}