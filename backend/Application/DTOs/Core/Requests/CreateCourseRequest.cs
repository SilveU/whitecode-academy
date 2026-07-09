namespace Application.DTOs.Core.Requests
{
    /// <summary>
    /// What the client sends. Never contains server-resolved fields (CurrentUserId, IsInstructor).
    /// </summary>
    public record CreateCourseRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal TotalHours { get; set; }
        public int TotalSections { get; set; }
        public Guid DepartmentId { get; set; }

        // Required only when the caller is Admin — ignored for Instructor callers
        public Guid? InstructorId { get; set; }
    }
}
