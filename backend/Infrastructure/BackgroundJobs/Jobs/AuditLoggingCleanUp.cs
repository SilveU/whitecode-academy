using Application.Interfaces.Repositories;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs.Jobs
{
    public class AuditLoggingCleanUp
    {
        private readonly IAuditLogRepository _auditRepository;
        private readonly ILogger<AuditLoggingCleanUp> _logger;
        private readonly IConfiguration _configuration;

        public AuditLoggingCleanUp(IAuditLogRepository auditRepository, ILogger<AuditLoggingCleanUp> logger, IConfiguration configuration)
        {
            _auditRepository = auditRepository;
            _logger = logger;
            _configuration = configuration;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Refresh token cleanup started");

            var deletedCount = await _auditRepository.DeleteExpiredAsync(_configuration.GetValue<int>("Audit:RetentionDays"));

            _logger.LogInformation("Refresh token cleanup finished. Deleted {Count} tokens", deletedCount);
        }
    }
}