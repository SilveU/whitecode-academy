using Microsoft.Extensions.Diagnostics.HealthChecks;
using nClam;

namespace API.Extentions.HealthChecks
{
    public class ClamAVHealthCheck : IHealthCheck
    {
        private readonly IClamClient _clam;
        private readonly ILogger<ClamAVHealthCheck> _logger;

        public ClamAVHealthCheck(IClamClient clam, ILogger<ClamAVHealthCheck> logger)
        {
            _logger = logger;
            _clam = clam;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if(!await _clam.PingAsync(cancellationToken))
                    return HealthCheckResult.Unhealthy("ClamAV service is not responding.");

                return HealthCheckResult.Healthy("ClamAV service is responding.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to communicate with ClamAV service.");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}