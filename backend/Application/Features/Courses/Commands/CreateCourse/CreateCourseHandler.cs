using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using Domain.Entites.Users;
using MediatR;

namespace Application.Features.Courses.Commands.CreateCourse
{
    public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, Result<CourseResponse>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public CreateCourseHandler(
            ICourseRepository courseRepository,
            IInstructorRepository instructorRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper)
        {
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
        }

        public async Task<Result<CourseResponse>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            Instructor instructor;

            if (request.IsInstructor)
            {
                var found = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (found == null)
                    return Result<CourseResponse>.NotFound("No instructor profile found for the current user.");
                instructor = found;
            }
            else
            {
                if (!request.InstructorId.HasValue)
                    return Result<CourseResponse>.Failure("InstructorId is required when creating a course as Admin.");

                var found = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.InstructorId.Value);
                if (found == null)
                    return Result<CourseResponse>.NotFound($"Instructor with ID {request.InstructorId} not found.");
                instructor = found;
            }

            if (instructor.Department == null || instructor.DepartmentId != request.DepartmentId)
                return Result<CourseResponse>.Failure("Instructor does not belong to the specified department.");

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

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = request.CurrentUserId,
                Action     = "Create",
                EntityName = nameof(Course),
                EntityId   = course.Id,
                OldValues  = null,
                NewValues  = AuditSerializer.Serialize(_mapper.Map<CourseResponse>(course)),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            return Result<CourseResponse>.Success(_mapper.Map<CourseResponse>(course), 201);
        }
    }
}
