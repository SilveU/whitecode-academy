using Application.Common;
using Application.DTOs.Core;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Departments.Commands.UpdateDepartment
{
    public record UpdateDepartmentCommand : IRequest<Result<DepartmentResponse>>
    {
        // Injected from the route
        public Guid? Id { get; init; }

        // Mapped from UpdateDepartmentRequest via AutoMapper
        public string? Name { get; set; }
        public string? Description { get; set; }

        // IFormFile — handler scans/uploads and stores the resulting path in Department.ImageUrl
        public IFormFile? ImageFile { get; set; }
    }
}
