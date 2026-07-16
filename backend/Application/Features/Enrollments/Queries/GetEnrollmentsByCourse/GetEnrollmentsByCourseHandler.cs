using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Enrollments.Queries.GetEnrollmentsByCourse
{
    public class GetEnrollmentsByCourseHandler : IRequestHandler<GetEnrollmentsByCourseQuery, Result<IEnumerable<EnrollmentResponse>>>
    {
        private readonly IConfiguration _configuration;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;

        public GetEnrollmentsByCourseHandler(IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository,
            IMapper mapper, ICacheService cache, IConfiguration configuration)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository     = courseRepository;
            _mapper               = mapper;
            _cache                = cache;
            _configuration        = configuration;
        }

        public async Task<Result<IEnumerable<EnrollmentResponse>>> Handle(GetEnrollmentsByCourseQuery request, CancellationToken cancellationToken)
        {
            var redisKey = $"{CacheKeys.EnrollmentsByCoursePrefix(request.CourseId)}:all";
            var cached   = await _cache.GetAsync<IEnumerable<EnrollmentResponse>>(redisKey);

            if (cached is not null)
                return Result<IEnumerable<EnrollmentResponse>>.Success(cached);

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
                return Result<IEnumerable<EnrollmentResponse>>.NotFound($"Course with ID {request.CourseId} not found.");

            var enrollments = await _enrollmentRepository.GetByCourseIdAsync(request.CourseId);
            var response    = _mapper.Map<IEnumerable<EnrollmentResponse>>(enrollments);

            await _cache.SetAsync(redisKey, response,
                TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:EnrollmentsExpirationMinutes")));

            return Result<IEnumerable<EnrollmentResponse>>.Success(response);
        }
    }
}
