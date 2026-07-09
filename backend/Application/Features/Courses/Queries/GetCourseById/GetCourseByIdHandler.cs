using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourseById
{
    public class GetCourseByIdHandler : IRequestHandler<GetCourseByIdQuery, Result<CourseResponse>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetCourseByIdHandler(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<Result<CourseResponse>> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (course == null)
                return Result<CourseResponse>.NotFound($"Course with ID {request.Id} not found.");

            return Result<CourseResponse>.Success(_mapper.Map<CourseResponse>(course));
        }
    }
}
