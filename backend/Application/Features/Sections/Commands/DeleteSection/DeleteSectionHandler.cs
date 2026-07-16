using Application.Common;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Sections.Commands.DeleteSection
{
    public class DeleteSectionHandler : IRequestHandler<DeleteSectionCommand, Result<bool>>
    {
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteSectionHandler> _logger;
        private readonly ISectionRepository _sectionRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAuditLogRepository _auditLogRepository;

        public DeleteSectionHandler(
            ISectionRepository sectionRepository,
            IInstructorRepository instructorRepository,
            IFileStorageService fileStorageService,
            IAuditLogRepository auditLogRepository,
            ILogger<DeleteSectionHandler> logger,
            ICacheService cache)
        {
            _sectionRepository    = sectionRepository;
            _instructorRepository = instructorRepository;
            _fileStorageService   = fileStorageService;
            _auditLogRepository   = auditLogRepository;
            _logger               = logger;
            _cache                = cache;
        }

        public async Task<Result<bool>> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
        {
            var section = await _sectionRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (section == null)
            {
                _logger.LogWarning("Section {SectionId} was not found.", request.Id);
                return Result<bool>.NotFound($"Section with ID {request.Id} not found.");
            }

            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                {
                    _logger.LogWarning("Instructor profile for user {UserId} was not found.", request.CurrentUserId);
                    return Result<bool>.NotFound("No instructor profile found for the current user.");
                }

                if (section.Course.InstructorId != instructor.Id)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to delete section {SectionId} without ownership.",
                        request.CurrentUserId, request.Id);
                    return Result<bool>.Forbidden("You can only delete sections of your own courses.");
                }
            }

            if (!string.IsNullOrEmpty(section.VideoUrl))
                await _fileStorageService.DeleteAsync(section.VideoUrl);

            if (!string.IsNullOrEmpty(section.PdfUrl))
                await _fileStorageService.DeleteAsync(section.PdfUrl);

            _sectionRepository.Delete(section);
            await _sectionRepository.SaveChangesAsync();

            await _cache.RemoveAsync(CacheKeys.Section(section.Id));
            await _cache.RemoveByPrefixAsync(CacheKeys.SectionsByCoursePrefix(section.CourseId));
            // Also bust the course cache since TotalSections / TotalDuration changed
            await _cache.RemoveAsync(CacheKeys.Course(section.CourseId));
            await _cache.RemoveByPrefixAsync(CacheKeys.CoursesPrefix());

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = request.CurrentUserId,
                Action     = "Delete",
                EntityName = nameof(Section),
                EntityId   = section.Id,
                OldValues  = null,
                NewValues  = null,
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation(
                "Section {SectionId} deleted successfully by user {UserId}.",
                request.Id, request.CurrentUserId);

            return Result<bool>.Success(true);
        }
    }
}
