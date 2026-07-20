using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using Domain.Entites.Users;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Courses.Commands.CreateCourse
{
    public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, Result<CourseResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly ILogger<CreateCourseHandler> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public CreateCourseHandler(
            ICourseRepository courseRepository,
            IInstructorRepository instructorRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ILogger<CreateCourseHandler> logger,
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

        public async Task<Result<CourseResponse>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            Instructor instructor;

            if (request.IsInstructor)
            {
                var found = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (found == null)
                {
                    _logger.LogWarning("Instructor profile for user {UserId} was not found.", request.CurrentUserId);
                    return Result<CourseResponse>.NotFound("No instructor profile found for the current user.");
                }
                instructor = found;
            }
            else
            {
                if (!request.InstructorId.HasValue)
                {
                    _logger.LogWarning("Course creation failed because InstructorId was not provided by admin.");
                    return Result<CourseResponse>.Failure("InstructorId is required when creating a course as Admin.");
                }

                var found = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.InstructorId.Value);
                if (found == null)
                {
                    _logger.LogWarning("Instructor {InstructorId} was not found.", request.InstructorId);
                    return Result<CourseResponse>.NotFound($"Instructor with ID {request.InstructorId} not found.");
                }
                instructor = found;
            }

            if (instructor.Department == null || instructor.DepartmentId != request.DepartmentId)
            {
                _logger.LogWarning("Instructor {InstructorId} does not belong to department {DepartmentId}.",
                instructor.Id, request.DepartmentId);
                return Result<CourseResponse>.Failure("Instructor does not belong to the specified department.");
            }

            var course = _mapper.Map<Course>(request);
            course.InstructorId = instructor.Id;
            course.CreatedAt = DateTimeOffset.UtcNow;
            course.TotalDurationInSeconds = 0;
            course.TotalSections = 0;

            instructor.Courses ??= new List<Course>();
            instructor.Courses.Add(course);

            instructor.Department.Courses ??= new List<Course>();
            instructor.Department.Courses.Add(course);

            await _courseRepository.CreateAsync(course);
            await _courseRepository.SaveChangesAsync();

            var response = _mapper.Map<CourseResponse>(course);

            await _cache.RemoveByPrefixAsync(CacheKeys.CoursesPrefix());

            var redisKey = CacheKeys.Course(course.Id);

            await _cache.SetAsync<CourseResponse>(redisKey, response,
            TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:CourseExpirationMinutes")));

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = request.CurrentUserId,
                Action = "Create",
                EntityName = nameof(Course),
                EntityId = course.Id,
                OldValues = null,
                NewValues = Serializer.Serialize(response),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation("Course {CourseId} created successfully by user {UserId}.", course.Id, request.CurrentUserId);

            return Result<CourseResponse>.Success(response, 201);
        }
    }
}
