namespace Application.DTOs.Core.Requests
{
    public record UpdateInstructorRequest
    {
        public Guid? DepartmentId { get; set; }
    }
}
