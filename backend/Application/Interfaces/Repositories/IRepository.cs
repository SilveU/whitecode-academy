using Domain.Entites.System;

namespace Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task CreateAsync(T entity);
        Task<int> SaveChangesAsync();
        Task<T?> GetByIdAsync(Guid id);
    }

    public interface IIdempotencyRepository : IRepository<Idempotency>
    {
        Task<Idempotency?> GetAsync(string? userId, string httpMethod, string path, string idempotencyKey);
        void Delete(Idempotency idempotency);
        Task DeleteExpiredAsync();
        Task<List<Idempotency>> GetExpiredAsync();
    }
}