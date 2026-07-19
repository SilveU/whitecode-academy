using Application.Common;
using Domain.Entites.Core;

namespace Application.Interfaces.Repositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<IEnumerable<Course>> SearchAsync(QueryParameters query);
        void Update(Course course);
        void Delete(Course course);
        Task<bool> HasActiveEnrollmentsAsync(Guid courseId);
    }
}