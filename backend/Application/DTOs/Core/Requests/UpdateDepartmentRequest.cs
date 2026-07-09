using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Core.Requests
{
    /// <summary>
    /// Multipart/form-data request. Bound with [FromForm] in the controller.
    /// All fields are optional — only provided values will be updated.
    /// </summary>
    public record UpdateDepartmentRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
