using Application.Interfaces.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs.Jobs
{
    public class IdempotencyCleanUpJob
    {
        private readonly IIdempotencyRepository _idempoRepository;
        private readonly ILogger<IdempotencyCleanUpJob> _logger;

        public IdempotencyCleanUpJob(IIdempotencyRepository idempoRepository, ILogger<IdempotencyCleanUpJob> logger)
        {
            _idempoRepository = idempoRepository;
            _logger = logger;
        }
        
        [AutomaticRetry(Attempts = 3)]
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Refresh token cleanup started");

            var deletedCount = await _idempoRepository.DeleteExpiredAsync();

            _logger.LogInformation("Refresh token cleanup finished. Deleted {Count} tokens", deletedCount);
        }
    }
}