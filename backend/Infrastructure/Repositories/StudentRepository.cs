using Application.Interfaces.Repositories;
using Domain.Entites.Users;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Delete(Student student)
        {
            student.IsDeleted = true;
            student.DeletedAt = DateTimeOffset.UtcNow;
        }

        public void Update(Student student)
        {
            student.UpdatedAt = DateTimeOffset.UtcNow;
        }

        public async Task<Student?> GetByIdWithNavigationPropertiesAsync(Guid id)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Enrollments.Where(e => !e.IsDeleted))
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
        }
    }
}
