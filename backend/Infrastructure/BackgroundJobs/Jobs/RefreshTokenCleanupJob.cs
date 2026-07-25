using Application.Interfaces.Authentecation;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs.Jobs
{
    public class RefreshTokenCleanupJob
    {
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<RefreshTokenCleanupJob> _logger;

        public RefreshTokenCleanupJob(IRefreshTokenService refreshTokenService, ILogger<RefreshTokenCleanupJob> logger)
        {
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Refresh token cleanup started");

            var deletedCount = await _refreshTokenService.CleanupAsync();

            _logger.LogInformation("Refresh token cleanup finished. Deleted {Count} tokens", deletedCount);
        }
    }
}