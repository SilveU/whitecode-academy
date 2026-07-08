using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Departments.Commands.UpdateDepartment
{
    public record UpdateDepartmentCommand : IRequest<Result<DepartmentResponse>>
    {
        public Guid? Id { get; init; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
