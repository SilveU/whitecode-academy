using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Instructors.Queries.GetInstructors
{
    public class GetInstructorsHandler : IRequestHandler<GetInstructorsQuery, Result<IEnumerable<InstructorResponse>>>
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;

        public GetInstructorsHandler(IInstructorRepository instructorRepository, IMapper mapper)
        {
            _instructorRepository = instructorRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<InstructorResponse>>> Handle(GetInstructorsQuery request, CancellationToken cancellationToken)
        {
            var instructors = await _instructorRepository.SearchAsync(request.Parameters);
            return Result<IEnumerable<InstructorResponse>>.Success(_mapper.Map<IEnumerable<InstructorResponse>>(instructors));
        }
    }
}
