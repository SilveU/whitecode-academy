namespace API.Extentions
{
    public static class CORSExtention
    {
        public static void AddCORSExtention(this IServiceCollection service, IConfiguration configuration, string corsPolicy)
        {
            // 6. Configure CORS policy
            var allowedOrigins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            service.AddCors(options =>
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
        }
    }
}