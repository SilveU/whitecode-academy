using Application.Common;
using Domain.Entites.Users;

namespace Application.Interfaces.Repositories
{
    public interface IInstructorRepository : IRepository<Instructor>
    {
        Task<IEnumerable<Instructor>> SearchAsync(QueryParameters query, CancellationToken cancellationToken = default);
        Task<Instructor?> GetByIdWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Instructor?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        void Update(Instructor instructor);
        void Delete(Instructor instructor);
    }
}