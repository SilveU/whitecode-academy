using Domain.Common;
using Domain.Entites.Users;

namespace Domain.Entites.Core
{
    public class Course : SoftDeletableEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public long TotalDurationInSeconds { get; set; }
        public int TotalSections { get; set; }

        public Guid InstructorId { get; set; }
        public Instructor Instructor { get; set; } = null!;
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
        public ICollection<Section>? Sections { get; set; }
    }
}