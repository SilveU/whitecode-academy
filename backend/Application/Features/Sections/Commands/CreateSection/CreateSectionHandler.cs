using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Core;
using MediatR;

namespace Application.Features.Sections.Commands.CreateSection
{
    public class CreateSectionHandler : IRequestHandler<CreateSectionCommand, Result<SectionResponse>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;

        public CreateSectionHandler(
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

        public async Task<Result<SectionResponse>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdWithNavigationPropertiesAsync(request.CourseId);
            if (course == null)
                return Result<SectionResponse>.NotFound($"Course with ID {request.CourseId} not found.");

            // Ownership check — an Instructor can only add sections to their own courses
            if (request.IsInstructor)
            {
                var instructor = await _instructorRepository.GetByUserIdAsync(request.CurrentUserId);
                if (instructor == null)
                    return Result<SectionResponse>.NotFound("No instructor profile found for the current user.");

                if (course.InstructorId != instructor.Id)
                    return Result<SectionResponse>.Forbidden("You can only add sections to your own courses.");
            }

            if (request.StartAt >= request.EndAt)
                return Result<SectionResponse>.Failure("Start time must be before end time.");

            var section = _mapper.Map<Section>(request);
            section.CreatedAt = DateTimeOffset.UtcNow;

            await _sectionRepository.CreateAsync(section);
            await _sectionRepository.SaveChangesAsync();

            return Result<SectionResponse>.Success(_mapper.Map<SectionResponse>(section), 201);
        }
    }
}
