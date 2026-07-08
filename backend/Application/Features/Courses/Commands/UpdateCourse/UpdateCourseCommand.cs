using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Courses.Commands.UpdateCourse
{
    public record UpdateCourseCommand : IRequest<Result<CourseResponse>>
    {
        public Guid? Id { get; init; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? TotalHours { get; set; }
        public int? TotalSections { get; set; }
        public Guid? InstructorId { get; set; }
        public Guid? DepartmentId { get; set; }

        // Set by the controller from JWT — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public bool IsInstructor { get; set; }
    }
}
