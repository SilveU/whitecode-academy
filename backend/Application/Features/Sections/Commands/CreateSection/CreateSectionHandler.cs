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

namespace Application.Features.Sections.Commands.CreateSection
{
    public class CreateSectionHandler : IRequestHandler<CreateSectionCommand, Result<SectionResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly ILogger<CreateSectionHandler> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly ISectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public CreateSectionHandler(
            ISectionRepository sectionRepository,
            ICourseRepository courseRepository,
            IInstructorRepository instructorRepository,
            IFileStorageService fileStorageService,
            IFileSecurityService fileSecurityService,
            IAuditLogRepository auditLogRepository,
            IWebHostEnvironment environment,
            IMapper mapper,
            ILogger<CreateSectionHandler> logger,
            ICacheService cache,
            IConfiguration configuration)
        {
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
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

        public async Task<Result<SectionResponse>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdWithNavigationPropertiesAsync(request.CourseId, cancellationToken);
            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} was not found.", request.CourseId);
                return Result<SectionResponse>.NotFound(MessageKeys.Common.Course_NotFound);
            }

            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId, cancellationToken);
                if (instructor == null)
                {
                    _logger.LogWarning("Instructor profile for user {UserId} was not found.", request.CurrentUserId);
                    return Result<SectionResponse>.NotFound(MessageKeys.Common.Course_InstructorNotFound);
                }

                if (course.InstructorId != instructor.Id)
                {
                    _logger.LogWarning("User {UserId} attempted to add a section to course {CourseId} without ownership.",
                        request.CurrentUserId, request.CourseId);
                    return Result<SectionResponse>.Forbidden(MessageKeys.Common.Section_AccessDenied);
                }
            }

            var section = _mapper.Map<Section>(request);
            section.Id = Guid.NewGuid();

            if (request.PdfFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.PdfFile, cancellationToken);
                await _fileSecurityService.ScanAsync(request.PdfFile, cancellationToken);
                var pdfFolder = Path.Combine("Sections", section.Id.ToString(), "Pdfs");
                section.PdfUrl = await _fileStorageService.UploadAsync(request.PdfFile, pdfFolder);
            }

            await _fileSecurityService.ValidateVideoAsync(request.VideoFile, cancellationToken);
            await _fileSecurityService.ScanAsync(request.VideoFile, cancellationToken);
            var videoFolder = Path.Combine("Sections", section.Id.ToString(), "Videos");
            section.VideoUrl = await _fileStorageService.UploadAsync(request.VideoFile, videoFolder);

            var physicalVideoPath = Path.Combine(_environment.WebRootPath, section.VideoUrl);
            var mediaInfo = await FFProbe.AnalyseAsync(physicalVideoPath, cancellationToken: cancellationToken);

            section.StartAt = TimeOnly.FromDateTime(DateTime.UtcNow);
            section.EndAt = section.StartAt.Add(mediaInfo.Duration);
            section.DayOfWeek = DateTimeOffset.UtcNow.DayOfWeek;
            section.CreatedAt = DateTimeOffset.UtcNow;

            course.TotalDurationInSeconds += (long)mediaInfo.Duration.TotalSeconds;
            course.TotalSections += 1;

            await _sectionRepository.CreateAsync(section, cancellationToken);
            await _sectionRepository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<SectionResponse>(section);

            await _cache.RemoveAsync(CacheKeys.Course(course.Id), cancellationToken);
            await _cache.RemoveByPrefixAsync(CacheKeys.CoursesPrefix(), cancellationToken);
            await _cache.RemoveByPrefixAsync(CacheKeys.SectionsByCoursePrefix(course.Id), cancellationToken);

            var redisKey = CacheKeys.Section(section.Id);
            await _cache.SetAsync<SectionResponse>(redisKey, response,
                TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:SectionExpirationMinutes")),
                cancellationToken);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = request.CurrentUserId,
                Action = "Create",
                EntityName = nameof(Section),
                EntityId = section.Id,
                OldValues = null,
                NewValues = Serializer.Serialize(response),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            }, cancellationToken);

            _logger.LogInformation("Section {SectionId} created successfully in course {CourseId} by user {UserId}.",
                section.Id, request.CourseId, request.CurrentUserId);

            return Result<SectionResponse>.Success(response, 201);
        }
    }
}
