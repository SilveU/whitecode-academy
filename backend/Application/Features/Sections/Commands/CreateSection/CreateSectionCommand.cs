using Application.Common;
using Application.DTOs.Core;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Sections.Commands.CreateSection
{
    public record CreateSectionCommand : IRequest<Result<SectionResponse>>
    {
        // Mapped from CreateSectionRequest via AutoMapper
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IFormFile VideoFile { get; set; } = null!;
        public IFormFile? PdfFile { get; set; }
        public Guid CourseId { get; set; }

        // Injected by the controller from the JWT — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public bool IsInstructor { get; set; }
    }
}
