using Application.Common;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Sections.Commands.DeleteSection
{
    public class DeleteSectionHandler : IRequestHandler<DeleteSectionCommand, Result<bool>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IInstructorRepository _instructorRepository;

        public DeleteSectionHandler(ISectionRepository sectionRepository, IInstructorRepository instructorRepository)
        {
            _sectionRepository = sectionRepository;
            _instructorRepository = instructorRepository;
        }

        public async Task<Result<bool>> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
        {
            // Load with navigation so we can check course ownership
            var section = await _sectionRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (section == null)
                return Result<bool>.NotFound($"Section with ID {request.Id} not found.");

            // Ownership check — an Instructor can only delete sections of their own courses
            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<bool>.NotFound("No instructor profile found for the current user.");

                if (section.Course.InstructorId != instructor.Id)
                    return Result<bool>.Forbidden("You can only delete sections of your own courses.");
            }

            _sectionRepository.Delete(section);
            await _sectionRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
