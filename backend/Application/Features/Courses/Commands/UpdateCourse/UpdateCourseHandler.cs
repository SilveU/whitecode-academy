using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;

namespace Application.Features.Courses.Commands.UpdateCourse
{
    public class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand, Result<CourseResponse>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public UpdateCourseHandler(
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

        public async Task<Result<CourseResponse>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
                return Result<CourseResponse>.Failure("Id cannot be empty.", 400);

            var course = await _courseRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value);
            if (course == null)
                return Result<CourseResponse>.NotFound("Course not found.");

            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<CourseResponse>.NotFound("No instructor profile found for the current user.");

                if (course.InstructorId != instructor.Id)
                    return Result<CourseResponse>.Forbidden("You are not the owner of this course.");
            }

            // Snapshot old state before mutation (serialized to avoid tracking issues)
            var oldValues = AuditSerializer.Serialize(_mapper.Map<CourseResponse>(course));

            // Fill nulls from the existing course (partial update)
            request.InstructorId ??= course.InstructorId;
            request.DepartmentId ??= course.DepartmentId;
            request.Name ??= course.Name;
            request.Description ??= course.Description;

            var targetInstructor = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.InstructorId.Value);
            if (targetInstructor == null)
                return Result<CourseResponse>.NotFound($"Instructor with ID {request.InstructorId} not found.");

            if (targetInstructor.Department == null || targetInstructor.DepartmentId != request.DepartmentId)
                return Result<CourseResponse>.Failure("Instructor does not belong to the specified department.");

            course = _mapper.Map(request, course);
            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = request.CurrentUserId,
                Action = "Update",
                EntityName = nameof(Course),
                EntityId = course.Id,
                OldValues = oldValues,
                NewValues = AuditSerializer.Serialize(_mapper.Map<CourseResponse>(course)),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            });

            return Result<CourseResponse>.Success(_mapper.Map<CourseResponse>(course));
        }
    }
}
