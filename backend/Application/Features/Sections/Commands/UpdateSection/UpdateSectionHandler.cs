using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Sections.Commands.UpdateSection
{
    public class UpdateSectionHandler : IRequestHandler<UpdateSectionCommand, Result<SectionResponse>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;

        public UpdateSectionHandler(
            ISectionRepository sectionRepository,
            ICourseRepository courseRepository,
            IInstructorRepository instructorRepository,
            IMapper mapper)
        {
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
            _instructorRepository = instructorRepository;
            _mapper = mapper;
        }

        public async Task<Result<SectionResponse>> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
                return Result<SectionResponse>.Failure("Id cannot be empty.", 400);

            // Load the section with its course so we can do the ownership check
            var section = await _sectionRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value);
            if (section == null)
                return Result<SectionResponse>.NotFound($"Section with ID {request.Id} not found.");

            // Ownership check — an Instructor can only update sections of their own courses
            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<SectionResponse>.NotFound("No instructor profile found for the current user.");

                if (section.Course.InstructorId != instructor.Id)
                    return Result<SectionResponse>.Forbidden("You can only update sections of your own courses.");
            }

            if (!string.IsNullOrEmpty(request.Name))
                section.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Description))
                section.Description = request.Description;

            if (!string.IsNullOrEmpty(request.VideoUrl))
                section.VideoUrl = request.VideoUrl;

            if (request.PdfUrl != null)
                section.PdfUrl = request.PdfUrl;

            if (request.DayOfWeek.HasValue)
                section.DayOfWeek = request.DayOfWeek.Value;

            var newStart = request.StartAt ?? section.StartAt;
            var newEnd   = request.EndAt   ?? section.EndAt;

            if (newStart >= newEnd)
                return Result<SectionResponse>.Failure("Start time must be before end time.");

            section.StartAt = newStart;
            section.EndAt   = newEnd;

            _sectionRepository.Update(section);
            await _sectionRepository.SaveChangesAsync();

            return Result<SectionResponse>.Success(_mapper.Map<SectionResponse>(section));
        }
    }
}
