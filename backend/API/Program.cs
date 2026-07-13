using Domain.Entites.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Data.Seeding;
using API.Extentions;

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

            builder.Services.AddDependencyInjectionExtention(builder.Configuration);
            builder.Services.AddAuthenticationExtention(builder.Configuration);

            var corsPolicy = builder.Configuration.GetValue<string>("CORS:CorsPolicy")!;

            builder.Services.AddCORSExtention(builder.Configuration, corsPolicy);


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

                await AppSeeder.SeedAsync(roleManager, userManager, context, builder.Configuration);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(); 
            }

            app.UseHttpsRedirection();

            app.UseCors(corsPolicy);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
