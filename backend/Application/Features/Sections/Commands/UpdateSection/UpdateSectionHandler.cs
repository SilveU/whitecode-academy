using Application.Localization;
using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using FFMpegCore;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Sections.Commands.UpdateSection
{
    public class UpdateSectionHandler : IRequestHandler<UpdateSectionCommand, Result<SectionResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateSectionHandler> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ISectionRepository _sectionRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public UpdateSectionHandler(ISectionRepository sectionRepository, IInstructorRepository instructorRepository,
            IFileStorageService fileStorageService, IFileSecurityService fileSecurityService,
            IAuditLogRepository auditLogRepository, IWebHostEnvironment environment, IMapper mapper,
            ILogger<UpdateSectionHandler> logger, ICacheService cache, IConfiguration configuration)
        {
            _sectionRepository = sectionRepository;
            _instructorRepository = instructorRepository;
            _fileStorageService = fileStorageService;
            _fileSecurityService = fileSecurityService;
            _auditLogRepository = auditLogRepository;
            _environment = environment;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<Result<SectionResponse>> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
            {
                _logger.LogWarning("Update section failed: missing Id.");
                return Result<SectionResponse>.Failure("Id cannot be empty.", 400);
            }

            var section = await _sectionRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value, cancellationToken);
            if (section == null)
            {
                _logger.LogWarning("Section {SectionId} was not found.", request.Id);
                return Result<SectionResponse>.NotFound(MessageKeys.Common.Section_NotFound);
            }

            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId, cancellationToken);
                if (instructor == null)
                {
                    _logger.LogWarning("Instructor profile for user {UserId} was not found.", request.CurrentUserId);
                    return Result<SectionResponse>.NotFound(MessageKeys.Common.Course_InstructorNotFound);
                }

                if (section.Course.InstructorId != instructor.Id)
                {
                    _logger.LogWarning("User {UserId} attempted to update section {SectionId} without ownership.",
                        request.CurrentUserId, request.Id);
                    return Result<SectionResponse>.Forbidden(MessageKeys.Common.Section_AccessDenied);
                }
            }

            var oldValues = Serializer.Serialize(_mapper.Map<SectionResponse>(section));

            if (!string.IsNullOrEmpty(request.Name))
                section.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description))
                section.Description = request.Description;

            if (request.VideoFile != null)
            {
                await _fileSecurityService.ValidateVideoAsync(request.VideoFile, cancellationToken);
                await _fileSecurityService.ScanAsync(request.VideoFile, cancellationToken);

                if (!string.IsNullOrEmpty(section.VideoUrl))
                    await _fileStorageService.DeleteAsync(section.VideoUrl);

                var videoFolder = Path.Combine("Sections", section.Id.ToString(), "Videos");
                section.VideoUrl = await _fileStorageService.UploadAsync(request.VideoFile, videoFolder);

                var physicalVideoPath = Path.Combine(_environment.WebRootPath, section.VideoUrl);
                var mediaInfo = await FFProbe.AnalyseAsync(physicalVideoPath, cancellationToken: cancellationToken);

                var oldDuration = (long)(section.EndAt - section.StartAt).TotalSeconds;
                section.Course.TotalDurationInSeconds =
                    section.Course.TotalDurationInSeconds - oldDuration + (long)mediaInfo.Duration.TotalSeconds;

                section.StartAt = TimeOnly.FromDateTime(DateTime.UtcNow);
                section.EndAt = section.StartAt.Add(mediaInfo.Duration);
            }

            if (request.PdfFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.PdfFile, cancellationToken);
                await _fileSecurityService.ScanAsync(request.PdfFile, cancellationToken);

                if (!string.IsNullOrEmpty(section.PdfUrl))
                    await _fileStorageService.DeleteAsync(section.PdfUrl);

                var pdfFolder = Path.Combine("Sections", section.Id.ToString(), "Pdfs");
                section.PdfUrl = await _fileStorageService.UploadAsync(request.PdfFile, pdfFolder);
            }

            _sectionRepository.Update(section);
            await _sectionRepository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<SectionResponse>(section);

            await _cache.RemoveAsync(CacheKeys.Section(section.Id), cancellationToken);
            await _cache.RemoveByPrefixAsync(CacheKeys.SectionsByCoursePrefix(section.CourseId), cancellationToken);

            await _cache.SetAsync<SectionResponse>(CacheKeys.Section(section.Id), response,
                TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:SectionExpirationMinutes")),
                cancellationToken);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = request.CurrentUserId,
                Action = "Update",
                EntityName = nameof(Section),
                EntityId = section.Id,
                OldValues = oldValues,
                NewValues = Serializer.Serialize(response),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            }, cancellationToken);

            _logger.LogInformation("Section {SectionId} updated successfully by user {UserId}.", section.Id, request.CurrentUserId);

            return Result<SectionResponse>.Success(response);
        }
    }
}
