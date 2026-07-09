using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourses
{
    public class GetCoursesHandler : IRequestHandler<GetCoursesQuery, Result<IEnumerable<CourseResponse>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetCoursesHandler(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<CourseResponse>>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
        {
            var courses = await _courseRepository.SearchAsync(request.Parameters);
            return Result<IEnumerable<CourseResponse>>.Success(_mapper.Map<IEnumerable<CourseResponse>>(courses));
        }
    }
}
