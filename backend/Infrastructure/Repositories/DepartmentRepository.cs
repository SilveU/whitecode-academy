using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entites.Core;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Delete(Department department)
        {
            department.IsDeleted = true;
            department.DeletedAt = DateTimeOffset.UtcNow;
        }

        public void Update(Department department)
        {
            department.UpdatedAt = DateTimeOffset.UtcNow;
        }

        public async Task<Department?> GetByIdWithNavigationPropertiesAsync(Guid id)
        {
            return await _context.Departments
                .Include(d => d.Courses.Where(c => !c.IsDeleted))
                .Include(d => d.Instructors.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<IEnumerable<Department>> SearchAsync(QueryParameters query)
        {
            var queryable = _context.Departments
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            return await ApplyQueryParameters(queryable, query);
        }

        public async Task<bool> HasActiveCoursesOrInstructorsAsync(Guid departmentId)
        {
            var hasActiveCourses = await _context.Courses
                .AnyAsync(c => c.DepartmentId == departmentId && !c.IsDeleted);

            if (hasActiveCourses) return true;

            var hasActiveInstructors = await _context.Instructors
                .AnyAsync(i => i.DepartmentId == departmentId && !i.IsDeleted);

            return hasActiveInstructors;
        }

        private async Task<IEnumerable<Department>> ApplyQueryParameters(IQueryable<Department> query, QueryParameters queryParameters)
        {
            if (!string.IsNullOrEmpty(queryParameters.WordForSearch))
            {
                var searchTerm = $"%{queryParameters.WordForSearch.Trim().ToLower()}%";
                query = query.Where(d =>
                    EF.Functions.Like(d.Name, searchTerm) ||
                    EF.Functions.Like(d.Description, searchTerm));
            }

            switch (queryParameters.SortBy?.ToLower())
            {
                case "name":
                    query = query.OrderBy(d => d.Name);
                    break;
                default:
                    query = query.OrderBy(d => d.Name);
                    break;
            }

            int skip = (queryParameters.PageNumber - 1) * queryParameters.PageSize;
            query = query.Skip(skip).Take(queryParameters.PageSize);

            return await query.ToListAsync();
        }
    }
}
