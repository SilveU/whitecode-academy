using Domain.Common;
using Domain.Entites.Users;

namespace Domain.Entites.Core
{
    public class Enrollment : SoftDeletableEntity
    {
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}
