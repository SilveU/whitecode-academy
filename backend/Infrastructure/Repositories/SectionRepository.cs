using Application.Interfaces.Repositories;
using Domain.Entites.Core;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SectionRepository : GenericRepository<Section>, ISectionRepository
    {
        private readonly ApplicationDbContext _context;

        public SectionRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Delete(Section section)
        {
            section.IsDeleted = true;
            section.DeletedAt = DateTimeOffset.UtcNow;
        }

        public void Update(Section section)
        {
            section.UpdatedAt = DateTimeOffset.UtcNow;
        }

        public async Task<Section?> GetByIdWithNavigationPropertiesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Sections
                .Include(s => s.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Section>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            return await _context.Sections
                .Where(s => s.CourseId == courseId && !s.IsDeleted)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartAt)
                .ToListAsync(cancellationToken);
        }
    }
}
