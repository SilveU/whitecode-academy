using Application.Common;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Courses.Commands.DeleteCourse
{
    public class DeleteCourseHandler : IRequestHandler<DeleteCourseCommand, Result<bool>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;

        public DeleteCourseHandler(ICourseRepository courseRepository, IInstructorRepository instructorRepository)
        {
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
        }

        public async Task<Result<bool>> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdAsync(request.Id);
            if (course == null)
                return Result<bool>.NotFound($"Course with ID {request.Id} not found.");

            // Ownership check — an Instructor can only delete their own courses
            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<bool>.NotFound("No instructor profile found for the current user.");

                if (course.InstructorId != instructor.Id)
                    return Result<bool>.Forbidden("You are not the owner of this course.");
            }

            var hasActiveEnrollments = await _courseRepository.HasActiveEnrollmentsAsync(request.Id);
            if (hasActiveEnrollments)
                return Result<bool>.Failure(
                    "Cannot delete this course because it has active enrollments. " +
                    "Please unenroll all students before deleting.", 409);

            _courseRepository.Delete(course);
            await _courseRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
