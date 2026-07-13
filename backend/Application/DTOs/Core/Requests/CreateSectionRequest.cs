using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Core.Requests
{
    /// <summary>
    /// Multipart/form-data request. Bound with [FromForm] in the controller.
    /// </summary>
    public record CreateSectionRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IFormFile VideoFile { get; set; } = null!;
        public IFormFile? PdfFile { get; set; }
        public Guid CourseId { get; set; }
    }
}
