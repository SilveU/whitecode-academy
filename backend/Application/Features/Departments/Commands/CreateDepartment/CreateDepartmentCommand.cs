using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Departments.Commands.CreateDepartment
{
    public record CreateDepartmentCommand : IRequest<Result<DepartmentResponse>>
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }
}
