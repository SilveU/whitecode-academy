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
        Task ValidatePdfAsync(IFormFile file);
        Task ValidateVideoAsync(IFormFile file);
        Task ScanAsync(IFormFile file);
    }
}