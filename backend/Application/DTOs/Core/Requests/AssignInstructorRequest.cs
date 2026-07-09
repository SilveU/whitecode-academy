namespace Application.DTOs.Core.Requests
{
    public record AssignInstructorRequest
    {
        public string UserId { get; set; } = null!;
        public Guid? DepartmentId { get; set; }
    }
}
