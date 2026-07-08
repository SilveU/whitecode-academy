namespace Application.DTOs.Core
{
    public record InstructorResponse
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
