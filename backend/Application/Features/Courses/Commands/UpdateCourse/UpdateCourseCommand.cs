using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Courses.Commands.UpdateCourse
{
    public record UpdateCourseCommand : IRequest<Result<CourseResponse>>
    {
        // Injected from the route
        public Guid? Id { get; init; }

        // Mapped from UpdateCourseRequest via AutoMapper
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? InstructorId { get; set; }
        public Guid? DepartmentId { get; set; }

        // Injected by the controller from the JWT — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public bool IsInstructor { get; set; }
    }
}
