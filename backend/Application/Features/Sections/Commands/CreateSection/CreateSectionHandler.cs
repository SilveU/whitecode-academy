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

namespace Application.Features.Sections.Commands.CreateSection
{
    public class CreateSectionHandler : IRequestHandler<CreateSectionCommand, Result<SectionResponse>>
    {
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
            IMapper mapper)
        {
            _sectionRepository  = sectionRepository;
            _courseRepository   = courseRepository;
            _instructorRepository = instructorRepository;
            _fileStorageService = fileStorageService;
            _fileSecurityService = fileSecurityService;
            _auditLogRepository = auditLogRepository;
            _environment        = environment;
            _mapper             = mapper;
        }

        public async Task<Result<SectionResponse>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdWithNavigationPropertiesAsync(request.CourseId);
            if (course == null)
                return Result<SectionResponse>.NotFound("Course not found.");

            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<SectionResponse>.NotFound("No instructor profile found for the current user.");

                if (course.InstructorId != instructor.Id)
                    return Result<SectionResponse>.Forbidden("You can only add sections to your own courses.");
            }

            var section = _mapper.Map<Section>(request);
            section.Id = Guid.NewGuid();

            if (request.PdfFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.PdfFile);
                await _fileSecurityService.ScanAsync(request.PdfFile);
                var pdfFolder = Path.Combine("Sections", section.Id.ToString(), "Pdfs");
                section.PdfUrl = await _fileStorageService.UploadAsync(request.PdfFile, pdfFolder);
            }

            await _fileSecurityService.ValidateVideoAsync(request.VideoFile);
            await _fileSecurityService.ScanAsync(request.VideoFile);
            var videoFolder = Path.Combine("Sections", section.Id.ToString(), "Videos");
            section.VideoUrl = await _fileStorageService.UploadAsync(request.VideoFile, videoFolder);

            var physicalVideoPath = Path.Combine(_environment.WebRootPath, section.VideoUrl);
            var mediaInfo = await FFProbe.AnalyseAsync(physicalVideoPath);

            section.StartAt  = TimeOnly.FromDateTime(DateTime.UtcNow);
            section.EndAt    = section.StartAt.Add(mediaInfo.Duration);
            section.DayOfWeek = DateTimeOffset.UtcNow.DayOfWeek;
            section.CreatedAt = DateTimeOffset.UtcNow;

            course.TotalDurationInSeconds += (long)mediaInfo.Duration.TotalSeconds;
            course.TotalSections          += 1;

            await _sectionRepository.CreateAsync(section);
            await _sectionRepository.SaveChangesAsync();

            var response = _mapper.Map<SectionResponse>(section);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = request.CurrentUserId,
                Action     = "Create",
                EntityName = nameof(Section),
                EntityId   = section.Id,
                OldValues  = null,
                NewValues  = AuditSerializer.Serialize(response),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            return Result<SectionResponse>.Success(response, 201);
        }
    }
}
