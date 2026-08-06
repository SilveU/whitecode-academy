using Application.Common;
using Domain.Entites.Core;

namespace Application.Interfaces.Repositories
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<Department?> GetByIdWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Department>> SearchAsync(QueryParameters query, CancellationToken cancellationToken = default);
        void Update(Department department);
        void Delete(Department department);
        Task<bool> HasActiveCoursesOrInstructorsAsync(Guid departmentId, CancellationToken cancellationToken = default);
    }
}