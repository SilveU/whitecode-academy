using Domain.Common;
using Domain.Entites.Users;

namespace Domain.Entites.Core
{
    public class Department : SoftDeletableEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public ICollection<Course> Courses { get; set; } = new HashSet<Course>();
        public ICollection<Instructor> Instructors { get; set; } = new HashSet<Instructor>();
    }
}
