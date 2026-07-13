using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _environment = env;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new BusinessRuleException("File is required.");

            if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
                throw new BusinessRuleException("Web root path is not configured.");

            var folderPath = Path.Combine(_environment.WebRootPath, folder);

            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var extention = Path.GetExtension(file.FileName);

            var originalName = Path.GetFileNameWithoutExtension(file.FileName);

            var samitizedFileName = originalName
            .Replace(" ", "-")
            .Replace("/", "")
            .Replace("\\", "");

            var fileName = $"{Guid.NewGuid()}_{samitizedFileName}{extention}";

            var fullPath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);

            await file.CopyToAsync(stream);

            return Path.Combine(folder, fileName).Replace("\\", "/");
        }

        public async Task DeleteAsync(string path)
        {
            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                throw new BusinessRuleException("Web root path is not configured.");

            var fullPath = Path.Combine(webRootPath, path);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}