using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Sections.Commands.CreateSection
{
    public record CreateSectionCommand : IRequest<Result<SectionResponse>>
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string VideoUrl { get; set; } = null!;
        public string? PdfUrl { get; set; }
        public TimeOnly StartAt { get; set; }
        public TimeOnly EndAt { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public Guid CourseId { get; set; }

        // Set by the controller from JWT — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public bool IsInstructor { get; set; }
    }
}
