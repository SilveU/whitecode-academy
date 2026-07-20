using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Extentions.HealthChecks
{
    public class LocalStorageHealthCheck : IHealthCheck
    {
        private readonly ILogger<LocalStorageHealthCheck> _logger;
        private readonly IWebHostEnvironment _env;

        public LocalStorageHealthCheck(IWebHostEnvironment env, ILogger<LocalStorageHealthCheck> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var path = _env.WebRootPath;
                if(string.IsNullOrEmpty(path))
                return HealthCheckResult.Unhealthy("WebRootPath is not configured."); 

                if(!Directory.Exists(path))
                    return HealthCheckResult.Unhealthy("Storage directory does not exist."); 

                var filePath = Path.Combine(path, "healthcheck.tmp");

                await using var stream = new FileStream(filePath, FileMode.Create);
                {
                    ReadOnlyMemory<byte> spanData = Encoding.UTF8.GetBytes("Check");
                    await stream.WriteAsync(spanData, cancellationToken);
                }

                File.Delete(filePath);
                return HealthCheckResult.Healthy("Local storage is available.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Local storage health check failed. Storage path: {StoragePath}", _env.WebRootPath);
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}