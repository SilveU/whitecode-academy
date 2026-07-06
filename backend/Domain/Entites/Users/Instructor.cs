using Domain.Common;
using Domain.Entites.Core;

namespace Domain.Entites.Users
{
    public class Instructor : SoftDeletableEntity
    {
        public ICollection<Course> Courses { get; set; } = new HashSet<Course>();

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
