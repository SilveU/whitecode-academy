using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder);

        Task DeleteAsync(string path);
    }
    public interface IFileSecurityService
    {
        Task ValidatePdfAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task ValidateImageAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task ValidateVideoAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task ScanAsync(IFormFile file, CancellationToken cancellationToken = default);
    }
}