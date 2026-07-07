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

        public Guid InstructorId { get; set; }
        public Guid DepartmentId { get; set; }
    }
}