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
        Task<Course?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<IEnumerable<Course>> SearchAsync(QueryParameters query);
        void Update(Course course);
        void Delete(Course course);
        Task<bool> HasActiveEnrollmentsAsync(Guid courseId);
    }

    public interface IInstructorRepository : IRepository<Instructor>
    {
        Task<IEnumerable<Instructor>> SearchAsync(QueryParameters query);
        Task<Instructor?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<Instructor?> GetByUserIdAsync(string userId);
        void Update(Instructor instructor);
        void Delete(Instructor instructor);
    }

    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<Department?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<IEnumerable<Department>> SearchAsync(QueryParameters query);
        void Update(Department department);
        void Delete(Department department);
        Task<bool> HasActiveCoursesOrInstructorsAsync(Guid departmentId);
    }

    public interface ISectionRepository : IRepository<Section>
    {
        Task<Section?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<IEnumerable<Section>> GetByCourseIdAsync(Guid courseId);
        void Update(Section section);
        void Delete(Section section);
    }

    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
        Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId);
        Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId);
        void Delete(Enrollment enrollment);
    }

    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student?> GetByIdWithNavigationPropertiesAsync(Guid id);
        Task<Student?> GetByUserIdAsync(string userId);
        void Update(Student student);
        void Delete(Student student);
    }
}