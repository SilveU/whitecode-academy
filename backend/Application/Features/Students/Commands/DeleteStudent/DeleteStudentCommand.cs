using Application.Common;
using MediatR;

namespace Application.Features.Students.Commands.DeleteStudent
{
    public record DeleteStudentCommand(Guid Id) : IRequest<Result<bool>>;
}
