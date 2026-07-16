using Domain.Entites.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Data.Seeding;
using API.Extentions;
using API.Middlewares;
using Serilog;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // run project -> dotnet run --launch-profile https 

            // setup the initial bootstrap logger
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

            Log.Information("Starting web application up...");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, lc) =>
            {
                lc.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information);
            });

            builder.Services.AddSwaggerGen();
            // Add services to the container.
            // 1. Fetch the connection string (usually from appsettings.json)

            builder.Services.AddDependencyInjectionExtention(builder.Configuration, builder.Environment);
            builder.Services.AddAuthenticationExtention(builder.Configuration);

            var corsPolicy = builder.Configuration.GetValue<string>("CORS:CorsPolicy")!;

            builder.Services.AddCORSExtention(builder.Configuration, corsPolicy);

            // dotnet ef migrations add InitialCreate --startup-project ../API

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddRateLimitConfiguration();

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

            app.UseSecurityHeaders();

            app.UseMiddleware<GlobalHandleExceptionMiddleware>();

            app.UseHttpsRedirection();

            // Enforce HTTPS (HSTS) - Highly recommended for production
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseCors(corsPolicy);

            app.UseStaticFiles();

            app.UseAuthentication();

            // Must run after UseAuthentication so context.User is populated
            app.UseMiddleware<TokenRevocationMiddleware>();
            
            app.UseMiddleware<IdempotencyMiddleware>();

            app.UseRateLimiter();

            app.UseAuthorization();

            // 3. Add clean HTTP request logging middleware
            app.UseSerilogRequestLogging();

            app.MapControllers();

            app.Run();
        }
    }
}
