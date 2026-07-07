namespace Application.DTOs.Core
{
    public record CourseResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public decimal TotalHours { get; set; }
        public int TotalSections { get; set; }

        public Guid InstructorId { get; set; }
        public Guid DepartmentId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    
    public record UpdateCourseCommand
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public decimal? TotalHours { get; set; }
        public int? TotalSections { get; set; }

        public Guid? InstructorId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}