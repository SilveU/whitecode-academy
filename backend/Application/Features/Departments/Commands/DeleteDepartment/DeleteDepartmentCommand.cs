using Application.Common;
using MediatR;

namespace Application.Features.Departments.Commands.DeleteDepartment
{
    public record DeleteDepartmentCommand(Guid Id) : IRequest<Result<bool>>;
}
