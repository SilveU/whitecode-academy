using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Sections.Commands.UpdateSection
{
    public record UpdateSectionCommand : IRequest<Result<SectionResponse>>
    {
        public Guid? Id { get; init; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public string? PdfUrl { get; set; }
        public TimeOnly? StartAt { get; set; }
        public TimeOnly? EndAt { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }

        // Set by the controller from JWT — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public bool IsInstructor { get; set; }
    }
}
