using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Instructors.Queries.GetInstructorById
{
    public class GetInstructorByIdHandler : IRequestHandler<GetInstructorByIdQuery, Result<InstructorResponse>>
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;

        public GetInstructorByIdHandler(IInstructorRepository instructorRepository, IMapper mapper)
        {
            _instructorRepository = instructorRepository;
            _mapper = mapper;
        }

        public async Task<Result<InstructorResponse>> Handle(GetInstructorByIdQuery request, CancellationToken cancellationToken)
        {
            var instructor = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (instructor == null)
                return Result<InstructorResponse>.NotFound($"Instructor with ID {request.Id} not found.");

            return Result<InstructorResponse>.Success(_mapper.Map<InstructorResponse>(instructor));
        }
    }
}
