using Application.Common;
using Domain.Entites.Users;

namespace Application.Interfaces.Repositories
{
    public interface IInstructorRepository : IRepository<Instructor>
    {
        Task<IEnumerable<Instructor>> SearchAsync(QueryParameters query);
        Task<Instructor?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<Instructor?> GetByUserIdAsync(string userId);
        void Update(Instructor instructor);
        void Delete(Instructor instructor);
    }
}