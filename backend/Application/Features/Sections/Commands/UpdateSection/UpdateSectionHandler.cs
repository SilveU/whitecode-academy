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
using Microsoft.Extensions.Logging;

namespace Application.Features.Sections.Commands.UpdateSection
{
    public class UpdateSectionHandler : IRequestHandler<UpdateSectionCommand, Result<SectionResponse>>
    {
        private readonly ILogger<UpdateSectionHandler> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ISectionRepository _sectionRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public UpdateSectionHandler(
            ISectionRepository sectionRepository,
            IInstructorRepository instructorRepository,
            IFileStorageService fileStorageService,
            IFileSecurityService fileSecurityService,
            IAuditLogRepository auditLogRepository,
            IWebHostEnvironment environment,
            IMapper mapper,
            ILogger<UpdateSectionHandler> logger)
        {
            _sectionRepository   = sectionRepository;
            _instructorRepository = instructorRepository;
            _fileStorageService  = fileStorageService;
            _fileSecurityService = fileSecurityService;
            _auditLogRepository  = auditLogRepository;
            _environment         = environment;
            _mapper              = mapper;
            _logger              = logger;
        }

        public async Task<Result<SectionResponse>> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
            {
                _logger.LogWarning("Update section failed: missing Id.");
                return Result<SectionResponse>.Failure("Id cannot be empty.", 400);
            }

            var section = await _sectionRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value);
            if (section == null)
            {
                _logger.LogWarning("Section {SectionId} was not found.", request.Id);
                return Result<SectionResponse>.NotFound($"Section with ID {request.Id} not found.");
            }

            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                {
                    _logger.LogWarning("Instructor profile for user {UserId} was not found.", request.CurrentUserId);
                    return Result<SectionResponse>.NotFound("No instructor profile found for the current user.");
                }

                if (section.Course.InstructorId != instructor.Id)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to update section {SectionId} without ownership.",
                        request.CurrentUserId, request.Id);
                    return Result<SectionResponse>.Forbidden("You can only update sections of your own courses.");
                }
            }

            var oldValues = AuditSerializer.Serialize(_mapper.Map<SectionResponse>(section));

            if (!string.IsNullOrEmpty(request.Name))        section.Name        = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) section.Description = request.Description;

            if (request.VideoFile != null)
            {
                await _fileSecurityService.ValidateVideoAsync(request.VideoFile);
                await _fileSecurityService.ScanAsync(request.VideoFile);

                if (!string.IsNullOrEmpty(section.VideoUrl))
                    await _fileStorageService.DeleteAsync(section.VideoUrl);

                var videoFolder = Path.Combine("Sections", section.Id.ToString(), "Videos");
                section.VideoUrl = await _fileStorageService.UploadAsync(request.VideoFile, videoFolder);

                var physicalVideoPath = Path.Combine(_environment.WebRootPath, section.VideoUrl);
                var mediaInfo = await FFProbe.AnalyseAsync(physicalVideoPath);

                var oldDuration = (long)(section.EndAt - section.StartAt).TotalSeconds;
                section.Course.TotalDurationInSeconds =
                    section.Course.TotalDurationInSeconds - oldDuration + (long)mediaInfo.Duration.TotalSeconds;

                section.StartAt = TimeOnly.FromDateTime(DateTime.UtcNow);
                section.EndAt   = section.StartAt.Add(mediaInfo.Duration);
            }

            if (request.PdfFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.PdfFile);
                await _fileSecurityService.ScanAsync(request.PdfFile);

                if (!string.IsNullOrEmpty(section.PdfUrl))
                    await _fileStorageService.DeleteAsync(section.PdfUrl);

                var pdfFolder = Path.Combine("Sections", section.Id.ToString(), "Pdfs");
                section.PdfUrl = await _fileStorageService.UploadAsync(request.PdfFile, pdfFolder);
            }

            _sectionRepository.Update(section);
            await _sectionRepository.SaveChangesAsync();

            var response = _mapper.Map<SectionResponse>(section);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = request.CurrentUserId,
                Action     = "Update",
                EntityName = nameof(Section),
                EntityId   = section.Id,
                OldValues  = oldValues,
                NewValues  = AuditSerializer.Serialize(response),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation(
                "Section {SectionId} updated successfully by user {UserId}.",
                section.Id, request.CurrentUserId);

            return Result<SectionResponse>.Success(response);
        }
    }
}
