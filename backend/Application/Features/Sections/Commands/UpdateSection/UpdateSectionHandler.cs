using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;

namespace Application.Features.Sections.Commands.UpdateSection
{
    public class UpdateSectionHandler : IRequestHandler<UpdateSectionCommand, Result<SectionResponse>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly IMapper _mapper;

        public UpdateSectionHandler(
            ISectionRepository sectionRepository,
            IInstructorRepository instructorRepository,
            IFileStorageService fileStorageService,
            IFileSecurityService fileSecurityService,
            IMapper mapper)
        {
            _sectionRepository   = sectionRepository;
            _instructorRepository = instructorRepository;
            _fileStorageService   = fileStorageService;
            _fileSecurityService  = fileSecurityService;
            _mapper               = mapper;
        }

        public async Task<Result<SectionResponse>> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
                return Result<SectionResponse>.Failure("Id cannot be empty.", 400);

            var section = await _sectionRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value);
            if (section == null)
                return Result<SectionResponse>.NotFound($"Section with ID {request.Id} not found.");

            // Ownership check
            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<SectionResponse>.NotFound("No instructor profile found for the current user.");

                if (section.Course.InstructorId != instructor.Id)
                    return Result<SectionResponse>.Forbidden("You can only update sections of your own courses.");
            }

            // Scalar field updates
            if (!string.IsNullOrEmpty(request.Name))        section.Name        = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) section.Description = request.Description;

            // PDF file replacement
            if (request.PdfFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.PdfFile);
                await _fileSecurityService.ScanAsync(request.PdfFile);

                if (!string.IsNullOrEmpty(section.PdfUrl))
                    await _fileStorageService.DeleteAsync(section.PdfUrl);

                var pdfFolder = Path.Combine("Sections", section.Id.ToString(), "Pdfs");
                section.PdfUrl = await _fileStorageService.UploadAsync(request.PdfFile, pdfFolder);
            }

            _sectionRepository.Update(section);
            await _sectionRepository.SaveChangesAsync();

            return Result<SectionResponse>.Success(_mapper.Map<SectionResponse>(section));
        }
    }
}
