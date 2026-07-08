using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entites.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Instructors.Commands.DeleteInstructor
{
    public class DeleteInstructorHandler : IRequestHandler<DeleteInstructorCommand, Result<bool>>
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteInstructorHandler(
            IInstructorRepository instructorRepository,
            ICourseRepository courseRepository,
            UserManager<ApplicationUser> userManager)
        {
            _instructorRepository = instructorRepository;
            _courseRepository = courseRepository;
            _userManager = userManager;
        }

        public async Task<Result<bool>> Handle(DeleteInstructorCommand request, CancellationToken cancellationToken)
        {
            var instructor = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (instructor == null)
                return Result<bool>.NotFound($"Instructor with ID {request.Id} not found.");

            // Block delete if the instructor has active courses
            var hasActiveCourses = instructor.Courses.Any(c => !c.IsDeleted);
            if (hasActiveCourses)
                return Result<bool>.Failure(
                    "Cannot remove this instructor because they have active courses assigned. " +
                    "Please reassign or delete those courses first.", 409);

            _instructorRepository.Delete(instructor);

            // Remove the Instructor role from the user
            var user = await _userManager.FindByIdAsync(instructor.UserId);
            if (user != null)
                await _userManager.RemoveFromRoleAsync(user, "Instructor");

            await _instructorRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
