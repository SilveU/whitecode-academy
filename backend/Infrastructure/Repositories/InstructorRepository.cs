using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entites.Users;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class InstructorRepository : GenericRepository<Instructor>, IInstructorRepository
    {
        private readonly ApplicationDbContext _context;
        public InstructorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void DeleteAsync(Instructor instructor)
        {
            instructor.IsDeleted = true;
            instructor.DeletedAt = DateTimeOffset.UtcNow;
        }

        public async Task<Instructor?> GetByIdWithNavigationPropertiesAsync(Guid id)
        {
            return await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Department)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<IEnumerable<Instructor>> SearchAsync(QueryParameters query)
        {
            var queryable = _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Department)
                .AsQueryable();

            return await ApplyQueryParameters(queryable, query);
        }

        private async Task<IQueryable<Instructor>> ApplyQueryParameters(IQueryable<Instructor> query, QueryParameters queryParameters)
        {
            // Apply search filter
            if (!string.IsNullOrEmpty(queryParameters.WordForSearch))
            {
                var searchTerm = $"%{queryParameters.WordForSearch.Trim().ToLower()}%";
                query = 
                query.Where(i => EF.Functions.Like(i.User.FirstName, searchTerm) ||
                EF.Functions.Like(i.User.LastName, searchTerm) ||
                EF.Functions.Like(i.Department!.Name, searchTerm));
            }

            // Apply sorting
            switch (queryParameters.SortBy?.ToLower())
            {
                case "firstname":
                    query = query.OrderBy(i => i.User.FirstName);
                    break;
                case "lastname":
                    query = query.OrderBy(i => i.User.LastName);
                    break;
                default:
                    query = query.OrderBy(i => i.User.FirstName); // Default sorting by first name
                    break;
            }

            return await Task.FromResult(query);
        }

        public async Task<Instructor> UpdateAsync(Instructor instructor)
        {
            instructor.UpdatedAt = DateTimeOffset.UtcNow;
            return instructor;
        }
    }
}