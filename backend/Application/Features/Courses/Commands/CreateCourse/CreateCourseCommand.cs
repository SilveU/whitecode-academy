using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Courses.Commands.CreateCourse
{
    public record CreateCourseCommand : IRequest<Result<CourseResponse>>
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal TotalHours { get; set; }
        public int TotalSections { get; set; }
        public Guid DepartmentId { get; set; }

        // Set by the controller from JWT — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public bool IsInstructor { get; set; }

        // Only used when Admin creates a course on behalf of an instructor
        public Guid? InstructorId { get; set; }
    }
}
