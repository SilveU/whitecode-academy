using Domain.Entites.Core;

namespace Application.Interfaces.Repositories
{
    public interface IEnrollmentRepository : IRepository<Enrollment>
    {
        Task<Enrollment?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId);
        Task<IEnumerable<Enrollment>> GetByStudentIdAsync(Guid studentId);
        Task<IEnumerable<Enrollment>> GetByCourseIdAsync(Guid courseId);
        void Delete(Enrollment enrollment);
    }
}