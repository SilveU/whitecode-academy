using Application.Common;
using MediatR;

namespace Application.Features.Enrollments.Commands.DeleteEnrollment
{
    public record DeleteEnrollmentCommand(Guid StudentId, Guid CourseId) : IRequest<Result<bool>>;
}
