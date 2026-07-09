using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Enrollments.Queries.GetEnrollmentsByCourse
{
    public class GetEnrollmentsByCourseHandler : IRequestHandler<GetEnrollmentsByCourseQuery, Result<IEnumerable<EnrollmentResponse>>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public GetEnrollmentsByCourseHandler(IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository, IMapper mapper)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository     = courseRepository;
            _mapper               = mapper;
        }

        public async Task<Result<IEnumerable<EnrollmentResponse>>> Handle(GetEnrollmentsByCourseQuery request, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
                return Result<IEnumerable<EnrollmentResponse>>.NotFound($"Course with ID {request.CourseId} not found.");

            var enrollments = await _enrollmentRepository.GetByCourseIdAsync(request.CourseId);
            return Result<IEnumerable<EnrollmentResponse>>.Success(_mapper.Map<IEnumerable<EnrollmentResponse>>(enrollments));
        }
    }
}
