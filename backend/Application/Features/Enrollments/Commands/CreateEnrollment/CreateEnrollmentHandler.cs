using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Enrollments.Commands.CreateEnrollment
{
    public class CreateEnrollmentHandler : IRequestHandler<CreateEnrollmentCommand, Result<EnrollmentResponse>>
    {
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
            ILogger<CreateEnrollmentHandler> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository    = studentRepository;
            _courseRepository     = courseRepository;
            _auditLogRepository   = auditLogRepository;
            _mapper               = mapper;
            _logger               = logger;
        }

        public async Task<Result<EnrollmentResponse>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var student = await _studentRepository.GetByUserIdAsync(request.CurrentUserId);
            if (student == null)
            {
                _logger.LogWarning("Student profile for user {UserId} was not found.", request.CurrentUserId);
                return Result<EnrollmentResponse>.NotFound("No student profile found for the current user.");
            }

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} was not found.", request.CourseId);
                return Result<EnrollmentResponse>.NotFound($"Course with ID {request.CourseId} not found.");
            }

            var existing = await _enrollmentRepository.GetByStudentAndCourseAsync(student.Id, request.CourseId);
            if (existing != null)
            {
                _logger.LogWarning(
                    "Student {StudentId} is already enrolled in course {CourseId}.",
                    student.Id, request.CourseId);
                return Result<EnrollmentResponse>.Failure("Student is already enrolled in this course.", 409);
            }

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId  = request.CourseId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _enrollmentRepository.CreateAsync(enrollment);
            await _enrollmentRepository.SaveChangesAsync();

            var response = _mapper.Map<EnrollmentResponse>(enrollment);
            response = response with { CourseName = course.Name };

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = request.CurrentUserId,
                Action     = "Create",
                EntityName = nameof(Enrollment),
                EntityId   = enrollment.Id,
                OldValues  = null,
                NewValues  = AuditSerializer.Serialize(response),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation(
                "Student {StudentId} enrolled in course {CourseId} successfully.",
                student.Id, request.CourseId);

            return Result<EnrollmentResponse>.Success(response, 201);
        }
    }
}
