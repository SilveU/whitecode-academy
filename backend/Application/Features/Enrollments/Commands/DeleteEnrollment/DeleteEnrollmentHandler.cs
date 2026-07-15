using Application.Common;
using Application.Helper;
using Application.Interfaces.Repositories;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Enrollments.Commands.DeleteEnrollment
{
    public class DeleteEnrollmentHandler : IRequestHandler<DeleteEnrollmentCommand, Result<bool>>
    {
        private readonly ILogger<DeleteEnrollmentHandler> _logger;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public DeleteEnrollmentHandler(
            IEnrollmentRepository enrollmentRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<DeleteEnrollmentHandler> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _auditLogRepository   = auditLogRepository;
            _logger               = logger;
        }

        public async Task<Result<bool>> Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(request.StudentId, request.CourseId);
            if (enrollment == null)
            {
                _logger.LogWarning(
                    "Enrollment for student {StudentId} in course {CourseId} was not found.",
                    request.StudentId, request.CourseId);
                return Result<bool>.NotFound("Enrollment not found for the given student and course.");
            }

            _enrollmentRepository.Delete(enrollment);
            await _enrollmentRepository.SaveChangesAsync();

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
