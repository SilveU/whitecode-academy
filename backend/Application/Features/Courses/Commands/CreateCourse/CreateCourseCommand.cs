using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Courses.Commands.CreateCourse
{
    public record CreateCourseCommand : IRequest<Result<CourseResponse>>
    {
        // Mapped from CreateCourseRequest via AutoMapper
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid DepartmentId { get; set; }
        public Guid? InstructorId { get; set; }

        // Injected by the controller from the JWT — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public bool IsInstructor { get; set; }
    }
}
