using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using nClam;

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

    public class ClamAvFileScanner : IFileSecurityService
    {
        private const long OneMB = 1024 * 1024;
        private readonly ClamClient _client;
        private readonly IConfiguration _configuration;

        public ClamAvFileScanner(IConfiguration configuration)
        {
            _configuration = configuration;

            var host = _configuration.GetValue<string>("ClamAV:Host")!;
            var port = _configuration.GetValue<int>("ClamAV:Port");

            _client = new ClamClient(host, port);
        }

        public async Task ScanAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BusinessRuleException("File is required.");

            await using var stream = file.OpenReadStream();

            var result = await _client.SendAndScanFileAsync(stream);

            if (result.Result == ClamScanResults.Clean)
                return;

            if (result.Result == ClamScanResults.VirusDetected)
                throw new BusinessRuleException("Uploaded file contains a virus.");

            throw new BusinessRuleException($"File scan failed: {result.RawResult}");
        }

        public async Task ValidatePdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BusinessRuleException("File is required.");

            var imagesAllowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            long maxSizeInBytes;

            if (!imagesAllowedExtensions.Contains(extension))
                throw new BusinessRuleException("Extention not Allowed");


            maxSizeInBytes = 5 * OneMB;

            if (file.Length > maxSizeInBytes)
                throw new BusinessRuleException("File size exceeds the allowed limit.");
        }

        public async Task ValidateVideoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BusinessRuleException("File is required.");

            var videoExtention = new[] { ".mp4" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            long maxSizeInBytes;

            if(!videoExtention.Contains(extension))
                throw new BusinessRuleException("Extention not Allowed");

            maxSizeInBytes = 1000 * OneMB;

            if (file.Length > maxSizeInBytes)
                throw new BusinessRuleException("File size exceeds the allowed limit.");
        }
    }
}