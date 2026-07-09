using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Departments.Queries.GetDepartments
{
    public record GetDepartmentsQuery(QueryParameters Parameters) : IRequest<Result<IEnumerable<DepartmentResponse>>>;
}
