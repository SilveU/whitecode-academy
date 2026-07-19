using Domain.Entites.Audits;

namespace Application.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task LogAsync(AuditLog auditLog);
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId);
        Task<IEnumerable<AuditLog>> GetByUserAsync(string userId);
    }
}