using Application.Interfaces.Services;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using nClam;

namespace Infrastructure.Services
{
    public class ClamAvFileScanner : IFileSecurityService
    {
        private const long OneMB = 1024 * 1024;
        private readonly IClamClient _client;

        public ClamAvFileScanner(IClamClient client)
        {
            _client = client;
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