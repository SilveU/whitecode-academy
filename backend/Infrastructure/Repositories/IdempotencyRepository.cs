using Application.Interfaces.Repositories;
using Domain.Entites.System;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class IdempotencyRepository : GenericRepository<Idempotency>, IIdempotencyRepository
    {
        private readonly ApplicationDbContext _context;

        public IdempotencyRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<Idempotency?> GetAsync(string? userId, string httpMethod, string path, string idempotencyKey)
        {
            return await _context.Idempotencies
                .FirstOrDefaultAsync(x =>
                    x.ExpiresAt <= DateTimeOffset.UtcNow &&
                    x.UserId == userId &&
                    x.HttpMethod == httpMethod &&
                    x.Path == path &&
                    x.IdempotencyKey == idempotencyKey);
        }

        public async Task<List<Idempotency>> GetExpiredAsync()
        {
            return await _context.Idempotencies
                .Where(x => x.ExpiresAt <= DateTimeOffset.UtcNow)
                .ToListAsync();
        }

        public void Delete(Idempotency idempotency)
        {
            _context.Idempotencies.Remove(idempotency);
        }
        public async Task<int> DeleteExpiredAsync()
        {
            return await _context.Idempotencies
                .Where(x => x.ExpiresAt <= DateTimeOffset.UtcNow)
                .ExecuteDeleteAsync();
        }
    }
}