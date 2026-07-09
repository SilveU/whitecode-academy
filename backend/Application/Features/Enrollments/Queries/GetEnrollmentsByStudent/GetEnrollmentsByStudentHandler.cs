using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Enrollments.Queries.GetEnrollmentsByStudent
{
    public class GetEnrollmentsByStudentHandler : IRequestHandler<GetEnrollmentsByStudentQuery, Result<IEnumerable<EnrollmentResponse>>>
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetEnrollmentsByStudentHandler(IEnrollmentRepository enrollmentRepository, IStudentRepository studentRepository, IMapper mapper)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository    = studentRepository;
            _mapper               = mapper;
        }

        public async Task<Result<IEnumerable<EnrollmentResponse>>> Handle(GetEnrollmentsByStudentQuery request, CancellationToken cancellationToken)
        {
            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
                return Result<IEnumerable<EnrollmentResponse>>.NotFound($"Student with ID {request.StudentId} not found.");

            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(request.StudentId);
            return Result<IEnumerable<EnrollmentResponse>>.Success(_mapper.Map<IEnumerable<EnrollmentResponse>>(enrollments));
        }
    }
}
