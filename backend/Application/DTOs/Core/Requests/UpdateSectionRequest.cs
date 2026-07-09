using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Core.Requests
{
    /// <summary>
    /// Multipart/form-data request. Bound with [FromForm] in the controller.
    /// All fields are optional — only provided values will be updated.
    /// </summary>
    public record UpdateSectionRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? VideoFile { get; set; }
        public IFormFile? PdfFile { get; set; }
        public TimeOnly? StartAt { get; set; }
        public TimeOnly? EndAt { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
    }
}
