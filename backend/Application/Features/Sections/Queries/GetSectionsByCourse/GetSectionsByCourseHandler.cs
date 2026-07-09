using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Sections.Queries.GetSectionsByCourse
{
    public class GetSectionsByCourseHandler : IRequestHandler<GetSectionsByCourseQuery, Result<IEnumerable<SectionResponse>>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetSectionsByCourseHandler(ISectionRepository sectionRepository, ICourseRepository courseRepository, IMapper mapper)
        {
            _sectionRepository = sectionRepository;
            _courseRepository  = courseRepository;
            _mapper            = mapper;
        }

        public async Task<Result<IEnumerable<SectionResponse>>> Handle(GetSectionsByCourseQuery request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
                return Result<IEnumerable<SectionResponse>>.NotFound($"Course with ID {request.CourseId} not found.");

            var sections = await _sectionRepository.GetByCourseIdAsync(request.CourseId);
            return Result<IEnumerable<SectionResponse>>.Success(_mapper.Map<IEnumerable<SectionResponse>>(sections));
        }
    }
}
