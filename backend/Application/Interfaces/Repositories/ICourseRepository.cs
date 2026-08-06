using Application.Common;
using Domain.Entites.Core;

namespace Application.Interfaces.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course?> GetByIdWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Course>> SearchAsync(QueryParameters query, CancellationToken cancellationToken = default);
        void Update(Course course);
        void Delete(Course course);
        Task<bool> HasActiveEnrollmentsAsync(Guid courseId, CancellationToken cancellationToken = default);
    }
}