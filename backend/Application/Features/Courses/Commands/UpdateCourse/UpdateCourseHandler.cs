using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Courses.Commands.UpdateCourse
{
    public class UpdateCourseHandler : IRequestHandler<UpdateCourseCommand, Result<CourseResponse>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;

        public UpdateCourseHandler(ICourseRepository courseRepository, IInstructorRepository instructorRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
            _mapper = mapper;
        }

        public async Task<Result<CourseResponse>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
                return Result<CourseResponse>.Failure("Id cannot be empty.", 400);

            var course = await _courseRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value);
            if (course == null)
                return Result<CourseResponse>.NotFound("Course not found.");

            // Ownership check — an Instructor can only update their own courses
            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<CourseResponse>.NotFound("No instructor profile found for the current user.");

                if (course.InstructorId != instructor.Id)
                    return Result<CourseResponse>.Forbidden("You are not the owner of this course.");
            }

            // Fill nulls from the existing course (partial update)
            request.InstructorId  ??= course.InstructorId;
            request.DepartmentId  ??= course.DepartmentId;
            request.Name          ??= course.Name;
            request.Description   ??= course.Description;
            request.TotalHours    ??= course.TotalHours;
            request.TotalSections ??= course.TotalSections;

            // Validate the instructor/department relationship if either changed
            var targetInstructor = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.InstructorId.Value);
            if (targetInstructor == null)
                return Result<CourseResponse>.NotFound($"Instructor with ID {request.InstructorId} not found.");

            if (targetInstructor.Department == null || targetInstructor.DepartmentId != request.DepartmentId)
                return Result<CourseResponse>.Failure("Instructor does not belong to the specified department.");

            if (request.TotalHours <= 0 || request.TotalSections < 0)
                return Result<CourseResponse>.Failure("Total hours must be greater than zero and total sections must be non-negative.");

            course = _mapper.Map(request, course);
            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            return Result<CourseResponse>.Success(_mapper.Map<CourseResponse>(course));
        }
    }
}
