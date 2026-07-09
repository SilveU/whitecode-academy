using Application.Common;
using Application.DTOs.Core;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Departments.Commands.CreateDepartment
{
    public record CreateDepartmentCommand : IRequest<Result<DepartmentResponse>>
    {
        // Mapped from CreateDepartmentRequest via AutoMapper
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        // IFormFile — handler scans/uploads and stores the resulting path in Department.ImageUrl
        public IFormFile? ImageFile { get; set; }
    }
}
