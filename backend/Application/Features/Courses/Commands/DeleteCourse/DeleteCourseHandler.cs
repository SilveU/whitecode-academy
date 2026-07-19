using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courses.Commands.DeleteCourse
{
    public class DeleteCourseHandler : IRequestHandler<DeleteCourseCommand, Result<bool>>
    {
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteCourseHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;

        public DeleteCourseHandler(ICourseRepository courseRepository, IInstructorRepository instructorRepository,
        ILogger<DeleteCourseHandler> logger, IAuditLogRepository auditLogRepository, IMapper mapper, ICacheService cache)
        {
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
            _logger = logger;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<Result<bool>> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdAsync(request.Id);

            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} was not found.", request.Id);

                return Result<bool>.NotFound($"Course with ID {request.Id} not found.");
            }

            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);

                if (instructor == null)
                {
                    _logger.LogWarning("Instructor profile for user {UserId} was not found.", request.CurrentUserId);

                    return Result<bool>.NotFound("No instructor profile found for the current user.");
                }

                if (course.InstructorId != instructor.Id)
                {
                    _logger.LogWarning("User {UserId} attempted to delete course {CourseId} without ownership.",
                    request.CurrentUserId, request.Id);

                    return Result<bool>.Forbidden("You are not the owner of this course.");
                }
            }

            var hasActiveEnrollments = await _courseRepository.HasActiveEnrollmentsAsync(request.Id);

            if (hasActiveEnrollments)
            {
                _logger.LogWarning("Course {CourseId} cannot be deleted because it has active enrollments.", request.Id);

                return Result<bool>.Failure("Cannot delete this course because it has active enrollments." +
                    " Please unenroll all students before deleting.", 409);
            }

            _courseRepository.Delete(course);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = "system",
                Action = "Delete",
                EntityName = nameof(Course),
                EntityId = course.Id,
                OldValues = Serializer.Serialize(_mapper.Map<CourseResponse>(course)),
                NewValues = null,
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            });

            await _courseRepository.SaveChangesAsync();

            await _cache.RemoveAsync(CacheKeys.Course(course.Id));
            await _cache.RemoveByPrefixAsync(CacheKeys.CoursesPrefix());


            _logger.LogInformation("Course {CourseId} was deleted successfully by user {UserId}.", request.Id, request.CurrentUserId);

            return Result<bool>.Success(true);
        }
    }
}
