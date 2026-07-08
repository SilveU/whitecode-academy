using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Students.Commands.AssignStudent
{
    // UserId is resolved from the JWT in the controller — not sent in the body
    public record AssignStudentCommand(string UserId) : IRequest<Result<StudentResponse>>;
}
