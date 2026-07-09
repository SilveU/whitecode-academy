using Application.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Sections.Commands.DeleteSection
{
    public class DeleteSectionHandler : IRequestHandler<DeleteSectionCommand, Result<bool>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IFileStorageService _fileStorageService;

        public DeleteSectionHandler(
            ISectionRepository sectionRepository,
            IInstructorRepository instructorRepository,
            IFileStorageService fileStorageService)
        {
            _sectionRepository    = sectionRepository;
            _instructorRepository = instructorRepository;
            _fileStorageService   = fileStorageService;
        }

        public async Task<Result<bool>> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
        {
            var section = await _sectionRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (section == null)
                return Result<bool>.NotFound($"Section with ID {request.Id} not found.");

            // Ownership check
            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<bool>.NotFound("No instructor profile found for the current user.");

                if (section.Course.InstructorId != instructor.Id)
                    return Result<bool>.Forbidden("You can only delete sections of your own courses.");
            }

            // Clean up stored files before soft-deleting the record
            if (!string.IsNullOrEmpty(section.VideoUrl))
                await _fileStorageService.DeleteAsync(section.VideoUrl);

            if (!string.IsNullOrEmpty(section.PdfUrl))
                await _fileStorageService.DeleteAsync(section.PdfUrl);

            _sectionRepository.Delete(section);
            await _sectionRepository.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
