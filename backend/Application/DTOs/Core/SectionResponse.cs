namespace Application.DTOs.Core
{
    public record SectionResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string VideoUrl { get; set; } = null!;
        public string? PdfUrl { get; set; }
        public TimeOnly StartAt { get; set; }
        public TimeOnly EndAt { get; set; }
        public string DayOfWeek { get; set; } = null!;
        public Guid CourseId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
