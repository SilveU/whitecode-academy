using Application.Common;
using Application.DTOs.Core;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Sections.Commands.UpdateSection
{
    public record UpdateSectionCommand : IRequest<Result<SectionResponse>>
    {
        // Injected from the route
        public Guid? Id { get; init; }

        // Mapped from UpdateSectionRequest via AutoMapper
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? VideoFile { get; set; }
        public IFormFile? PdfFile { get; set; }
        public TimeOnly? StartAt { get; set; }
        public TimeOnly? EndAt { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }

        // Injected by the controller from the JWT — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public bool IsInstructor { get; set; }
    }
}
