using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Core;
using MediatR;

namespace Application.Features.Enrollments.Commands.CreateEnrollment
{
    public class CreateEnrollmentHandler : IRequestHandler<CreateEnrollmentCommand, Result<EnrollmentResponse>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public CreateEnrollmentHandler(
            IEnrollmentRepository enrollmentRepository,
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IMapper mapper)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<Result<EnrollmentResponse>> Handle(CreateEnrollmentCommand request, CancellationToken cancellationToken)
        {
            // Resolve student profile from the authenticated user's identity
            var student = await _studentRepository.GetByUserIdAsync(request.CurrentUserId);
            if (student == null)
                return Result<EnrollmentResponse>.NotFound("No student profile found for the current user.");

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
                return Result<EnrollmentResponse>.NotFound($"Course with ID {request.CourseId} not found.");

            var existing = await _enrollmentRepository.GetByStudentAndCourseAsync(student.Id, request.CourseId);
            if (existing != null)
                return Result<EnrollmentResponse>.Failure("Student is already enrolled in this course.", 409);

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

            return Result<EnrollmentResponse>.Success(response, 201);
        }
    }
}
