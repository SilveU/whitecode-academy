using Domain.Entites.Users;

namespace Application.Interfaces.Repositories
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<Student?> GetByUserIdAsync(string userId);
        void Update(Student student);
        void Delete(Student student);
    }
}