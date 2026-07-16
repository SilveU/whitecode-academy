using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Sections.Queries.GetSectionsByCourse
{
    public class GetSectionsByCourseHandler : IRequestHandler<GetSectionsByCourseQuery, Result<IEnumerable<SectionResponse>>>
    {
        private readonly IConfiguration _configuration;
        private readonly ISectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;

        public GetSectionsByCourseHandler(ISectionRepository sectionRepository, ICourseRepository courseRepository,
            IMapper mapper, ICacheService cache, IConfiguration configuration)
        {
            _sectionRepository = sectionRepository;
            _courseRepository  = courseRepository;
            _mapper            = mapper;
            _cache             = cache;
            _configuration     = configuration;
        }

        public async Task<Result<IEnumerable<SectionResponse>>> Handle(GetSectionsByCourseQuery request, CancellationToken cancellationToken)
        {
            var redisKey = $"{CacheKeys.SectionsByCoursePrefix(request.CourseId)}:all";
            var cached   = await _cache.GetAsync<IEnumerable<SectionResponse>>(redisKey);

            if (cached is not null)
                return Result<IEnumerable<SectionResponse>>.Success(cached);

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
                return Result<IEnumerable<SectionResponse>>.NotFound($"Course with ID {request.CourseId} not found.");

            var sections = await _sectionRepository.GetByCourseIdAsync(request.CourseId);
            var response = _mapper.Map<IEnumerable<SectionResponse>>(sections);

            await _cache.SetAsync(redisKey, response,
                TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:SectionsExpirationMinutes")));

            return Result<IEnumerable<SectionResponse>>.Success(response);
        }
    }
}
