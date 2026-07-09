using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Departments.Queries.GetDepartmentById
{
    public record GetDepartmentByIdQuery(Guid Id) : IRequest<Result<DepartmentResponse>>;
}
