using Application.Localization;
using Application.Common;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Enrollments.Commands.DeleteEnrollment
{
    public class DeleteEnrollmentHandler : IRequestHandler<DeleteEnrollmentCommand, Result<bool>>
    {
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteEnrollmentHandler> _logger;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public DeleteEnrollmentHandler(
            IEnrollmentRepository enrollmentRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<DeleteEnrollmentHandler> logger,
            ICacheService cache)
        {
            _enrollmentRepository = enrollmentRepository;
            _auditLogRepository   = auditLogRepository;
            _logger               = logger;
            _cache                = cache;
        }

        public async Task<Result<bool>> Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(request.StudentId, request.CourseId);
            if (enrollment == null)
            {
                _logger.LogWarning(
                    "Enrollment for student {StudentId} in course {CourseId} was not found.",
                    request.StudentId, request.CourseId);
                return Result<bool>.NotFound(MessageKeys.Common.Enrollment_NotFound);
            }

            _enrollmentRepository.Delete(enrollment);
            await _enrollmentRepository.SaveChangesAsync();

            await _cache.RemoveByPrefixAsync(CacheKeys.EnrollmentsByCoursePrefix(request.CourseId));
            await _cache.RemoveByPrefixAsync(CacheKeys.EnrollmentsByStudentPrefix(request.StudentId));

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = "system",
                Action     = "Delete",
                EntityName = nameof(Enrollment),
                EntityId   = enrollment.Id,
                OldValues  = null,
                NewValues  = null,
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation(
                "Enrollment for student {StudentId} in course {CourseId} deleted successfully.",
                request.StudentId, request.CourseId);

            return Result<bool>.Success(true);
        }
    }
}
