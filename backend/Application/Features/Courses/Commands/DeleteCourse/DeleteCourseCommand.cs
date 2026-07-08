using Application.Common;
using MediatR;

namespace Application.Features.Courses.Commands.DeleteCourse
{
    public record DeleteCourseCommand(Guid Id, string CurrentUserId, bool IsInstructor) : IRequest<Result<bool>>;
}
