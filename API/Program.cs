using Domain.Entites.Users;
using Infrastructure.Data;
using Infrastructure.Data.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // 1. Fetch the connection string (usually from appsettings.json)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddSwaggerGen();

            // 2. Register your DbContext in the DI container
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 3. Register Identity services
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // dotnet ef migrations add InitialCreate --startup-project ../API

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // using (var scope = app.Services.CreateScope())
            // {
            //     var services = scope.ServiceProvider;

            //     var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            //     var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            //     await AppSeeder.SeedAsync(roleManager, userManager);
            // }

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
