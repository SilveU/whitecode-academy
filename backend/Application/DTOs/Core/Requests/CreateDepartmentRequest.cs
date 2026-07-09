using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Core.Requests
{
    /// <summary>
    /// Multipart/form-data request. Bound with [FromForm] in the controller.
    /// </summary>
    public record CreateDepartmentRequest
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IFormFile? ImageFile { get; set; }
    }
}
