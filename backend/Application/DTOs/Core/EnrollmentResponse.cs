namespace Application.DTOs.Core
{
    public record EnrollmentResponse
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
