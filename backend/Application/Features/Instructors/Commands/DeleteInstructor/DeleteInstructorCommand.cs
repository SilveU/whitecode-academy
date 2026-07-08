using Application.Common;
using MediatR;

namespace Application.Features.Instructors.Commands.DeleteInstructor
{
    public record DeleteInstructorCommand(Guid Id) : IRequest<Result<bool>>;
}
