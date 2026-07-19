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

namespace Application.Features.Courses.Commands.UpdateCourse
{
    public class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand, Result<CourseResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateCourseHandler> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public UpdateCourseHandler(
            ICourseRepository courseRepository,
            IInstructorRepository instructorRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ILogger<UpdateCourseHandler> logger,
            ICacheService cache,
            IConfiguration configuration)
        {
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<Result<CourseResponse>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
            {
                _logger.LogWarning(
                    "Update course failed: missing {CourseId}.",
                    request.Id);

                return Result<CourseResponse>.Failure("Id cannot be empty.", 400);
            }

            var course = await _courseRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value);

            if (course == null)
            {
                _logger.LogWarning(
                    "Course {CourseId} not found.",
                    request.Id);

                return Result<CourseResponse>.NotFound("Course not found.");
            }

            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);

                if (instructor == null)
                {
                    _logger.LogWarning(
                        "Instructor profile not found for user {UserId}.",
                        request.CurrentUserId);

                    return Result<CourseResponse>.NotFound("No instructor profile found for the current user.");
                }

                if (course.InstructorId != instructor.Id)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to update course {CourseId} without ownership.",
                        request.CurrentUserId,
                        course.Id);

                    return Result<CourseResponse>.Forbidden("You are not the owner of this course.");
                }
            }

            var oldValues = Serializer.Serialize(_mapper.Map<CourseResponse>(course));

            request.InstructorId ??= course.InstructorId;
            request.DepartmentId ??= course.DepartmentId;
            request.Name ??= course.Name;
            request.Description ??= course.Description;

            var targetInstructor = await _instructorRepository
                .GetByIdWithNavigationPropertiesAsync(request.InstructorId.Value);

            if (targetInstructor == null)
            {
                _logger.LogWarning(
                    "Instructor {InstructorId} not found.",
                    request.InstructorId);

                return Result<CourseResponse>.NotFound($"Instructor with ID {request.InstructorId} not found.");
            }

            if (targetInstructor.Department == null ||
                targetInstructor.DepartmentId != request.DepartmentId)
            {
                _logger.LogWarning(
                    "Instructor {InstructorId} does not belong to department {DepartmentId}.",
                    targetInstructor.Id,
                    request.DepartmentId);

                return Result<CourseResponse>.Failure("Instructor does not belong to the specified department.");
            }

            course = _mapper.Map(request, course);

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();
            
            var response = _mapper.Map<CourseResponse>(course);

            await _cache.RemoveAsync(CacheKeys.Course(course.Id));
            await _cache.RemoveByPrefixAsync(CacheKeys.CoursesPrefix());

            var redisKey = CacheKeys.Course(course.Id);

            await _cache.SetAsync<CourseResponse>(redisKey, response,
            TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:CourseExpirationMinutes")));

            _logger.LogInformation(
                "Course {CourseId} updated successfully by user {UserId}.",
                course.Id,
                request.CurrentUserId);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = request.CurrentUserId,
                Action = "Update",
                EntityName = nameof(Course),
                EntityId = course.Id,
                OldValues = oldValues,
                NewValues = Serializer.Serialize(_mapper.Map<CourseResponse>(course)),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            });

            return Result<CourseResponse>.Success(_mapper.Map<CourseResponse>(course));
        }
    }
}
