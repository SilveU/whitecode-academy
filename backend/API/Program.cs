using System.Text;
using API.Mapping;
using Application.Interfaces.Authentecation;
using Application.Interfaces.Services;
using Application.Validations;
using Domain.Entites.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Authentecation;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Data.Seeding;
using Application.Interfaces.Repositories;
using Infrastructure.Repositories;
using Application.Features.Courses.Commands.CreateCourse;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // run project -> dotnet run --launch-profile https 

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSwaggerGen();
            // Add services to the container.
            // 1. Fetch the connection string (usually from appsettings.json)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // 2. Register your DbContext in the DI container
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 3. Register Identity services
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // 4. Register Bearer token authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
                    ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:Key")!))
                };
            });

            // 6. Configure CORS policy
            var corsPolicy = builder.Configuration.GetValue<string>("CORS:CorsPolicy")!;
            var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(corsPolicy, policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // 5. Register Authentication services
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // 6. Register AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            // 7. Register FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

            // 8. Register other services
            builder.Services.AddScoped<IEmailSender, EmailSender>();

            // 9. Register MediatR and scan the assembly where the Program class lives
            builder.Services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(typeof(CreateCourseHandler).Assembly));

            // 10. Register repositories
            builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();

            // dotnet ef migrations add InitialCreate --startup-project ../API

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var context = services.GetRequiredService<ApplicationDbContext>();

                await AppSeeder.SeedAsync(roleManager, userManager, context);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(); 
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
