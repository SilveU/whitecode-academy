using Application.Localization;
using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Enrollments.Commands.CreateEnrollment
{
    public class CreateEnrollmentHandler : IRequestHandler<CreateEnrollmentCommand, Result<EnrollmentResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly ILogger<CreateEnrollmentHandler> _logger;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public CreateEnrollmentHandler(
            IEnrollmentRepository enrollmentRepository,
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ILogger<CreateEnrollmentHandler> logger,
            ICacheService cache,
            IConfiguration configuration)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<Result<EnrollmentResponse>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var student = await _studentRepository.GetByUserIdAsync(request.CurrentUserId, cancellationToken);
            if (student == null)
            {
                _logger.LogWarning("Student profile for user {UserId} was not found.", request.CurrentUserId);
                return Result<EnrollmentResponse>.NotFound(MessageKeys.Common.Student_NotFound);
            }

            var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} was not found.", request.CourseId);
                return Result<EnrollmentResponse>.NotFound(MessageKeys.Common.Course_NotFound);
            }

            var existing = await _enrollmentRepository.GetByStudentAndCourseAsync(student.Id, request.CourseId, cancellationToken);
            if (existing != null)
            {
                _logger.LogWarning("Student {StudentId} is already enrolled in course {CourseId}.", student.Id, request.CourseId);
                return Result<EnrollmentResponse>.Failure(MessageKeys.Common.Enrollment_AlreadyExists, 409);
            }

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = request.CourseId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _enrollmentRepository.CreateAsync(enrollment, cancellationToken);
            await _enrollmentRepository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<EnrollmentResponse>(enrollment);
            response = response with { CourseName = course.Name };

            await _cache.RemoveByPrefixAsync(CacheKeys.EnrollmentsByCoursePrefix(request.CourseId), cancellationToken);
            await _cache.RemoveByPrefixAsync(CacheKeys.EnrollmentsByStudentPrefix(student.Id), cancellationToken);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = request.CurrentUserId,
                Action = "Create",
                EntityName = nameof(Enrollment),
                EntityId = enrollment.Id,
                OldValues = null,
                NewValues = Serializer.Serialize(response),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            }, cancellationToken);

            _logger.LogInformation("Student {StudentId} enrolled in course {CourseId} successfully.", student.Id, request.CourseId);

            return Result<EnrollmentResponse>.Success(response, 201);
        }
    }
}
