using Application.Common;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Enrollments.Commands.DeleteEnrollment
{
    public class DeleteEnrollmentHandler : IRequestHandler<DeleteEnrollmentCommand, Result<bool>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;

        public DeleteEnrollmentHandler(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<Result<bool>> Handle(DeleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(request.StudentId, request.CourseId);
            if (enrollment == null)
                return Result<bool>.NotFound("Enrollment not found for the given student and course.");

            _enrollmentRepository.Delete(enrollment);
            await _enrollmentRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
