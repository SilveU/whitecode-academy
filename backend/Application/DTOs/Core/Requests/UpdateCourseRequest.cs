namespace Application.DTOs.Core.Requests
{
    public record UpdateCourseRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? TotalHours { get; set; }
        public int? TotalSections { get; set; }
        public Guid? InstructorId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}
