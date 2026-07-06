using Domain.Common;
using Domain.Entites.Core;

namespace Domain.Entites.Users
{
    public class Student : SoftDeletableEntity
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
    }
}
