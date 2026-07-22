using Application.Interfaces.Repositories;
using Domain.Entites.Audits;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(AuditLog auditLog)
        {
            auditLog.Id = Guid.NewGuid();
            auditLog.CreatedAt = DateTimeOffset.UtcNow;

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId)
            => await _context.AuditLogs
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId)
            => await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
