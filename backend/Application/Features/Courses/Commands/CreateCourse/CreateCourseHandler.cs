using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Core;
using Domain.Entites.Users;
using MediatR;

namespace Application.Features.Courses.Commands.CreateCourse
{
    public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, Result<CourseResponse>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;

        public CreateCourseHandler(ICourseRepository courseRepository, IMapper mapper, IInstructorRepository instructorRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _instructorRepository = instructorRepository;
        }

        public async Task<Result<CourseResponse>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            Instructor instructor;

            if (request.IsInstructor)
            {
                // The caller is an Instructor — resolve their profile from the JWT identity
                var found = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (found == null)
                    return Result<CourseResponse>.NotFound("No instructor profile found for the current user.");
                instructor = found;
            }
            else
            {
                // Admin creating a course on behalf of an instructor — InstructorId must be provided
                if (!request.InstructorId.HasValue)
                    return Result<CourseResponse>.Failure("InstructorId is required when creating a course as Admin.");

                var found = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.InstructorId.Value);
                if (found == null)
                    return Result<CourseResponse>.NotFound($"Instructor with ID {request.InstructorId} not found.");
                instructor = found;
            }

            if (instructor.Department == null || instructor.DepartmentId != request.DepartmentId)
                return Result<CourseResponse>.Failure("Instructor does not belong to the specified department.");

            if (request.TotalHours <= 0 || request.TotalSections < 0)
                return Result<CourseResponse>.Failure("Total hours must be greater than zero and total sections must be non-negative.");

            var course = _mapper.Map<Course>(request);
            course.InstructorId = instructor.Id;
            course.CreatedAt    = DateTimeOffset.UtcNow;

            instructor.Courses ??= new List<Course>();
            instructor.Courses.Add(course);

            instructor.Department.Courses ??= new List<Course>();
            instructor.Department.Courses.Add(course);

            await _courseRepository.CreateAsync(course);
            await _courseRepository.SaveChangesAsync();

            return Result<CourseResponse>.Success(_mapper.Map<CourseResponse>(course), 201);
        }
    }
}
