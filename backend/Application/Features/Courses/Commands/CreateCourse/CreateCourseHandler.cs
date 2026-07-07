using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Core;
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
            var instructor = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.InstructorId);
            if (instructor == null)
                return Result<CourseResponse>.Failure($"Instructor with ID {request.InstructorId} not found.");

            var department = instructor.Department;
            if (department == null || instructor.DepartmentId != request.DepartmentId)
                return Result<CourseResponse>.Failure("Instructor" +
                " does not belong to the specified department");

            if(request.TotalHours <= 0 && request.TotalSections >= 0)
                return Result<CourseResponse>.Failure("Total hours must be greater than zero and total sections must be non-negative.");

                
            var course = _mapper.Map<Course>(request);
            course.CreatedAt = DateTimeOffset.UtcNow;

            instructor.Courses ??= new List<Course>();
            instructor.Courses.Add(course);

            department.Courses ??= new List<Course>();
            department.Courses.Add(course);

            await _courseRepository.CreateAsync(course);

            await _courseRepository.SaveChangesAsync();

            return Result<CourseResponse>.Success(_mapper.Map<CourseResponse>(course));
        }
    }
}