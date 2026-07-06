using Domain.Common;

namespace Domain.Entites.Core
{
    public class Section : SoftDeletableEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string VideoUrl { get; set; } = null!;
        public string? PdfUrl { get; set; }
        public TimeOnly StartAt { get; set; }
        public TimeOnly EndAt { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}
