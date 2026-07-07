using Application.Common;
using Domain.Entites.Core;
using Domain.Entites.Users;

namespace Application.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task CreateAsync(T entity);
        Task<int> SaveChangesAsync();
        Task<T?> GetByIdAsync(Guid id);
    }

    public interface ICourseRepository : IRepository<Course>
    {
        Task<IEnumerable<Course>> SearchAsync(QueryParameters query);
        Task<Course> UpdateAsync(Course course);
        void DeleteAsync(Course course);
    }   

    public interface IInstructorRepository : IRepository<Instructor>
    {
        Task<IEnumerable<Instructor>> SearchAsync(QueryParameters query);
        Task<Instructor?> GetByIdWithNavigationPropertiesAsync(Guid id);
    }
}