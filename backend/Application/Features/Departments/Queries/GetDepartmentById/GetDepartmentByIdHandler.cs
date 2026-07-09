using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Departments.Queries.GetDepartmentById
{
    public class GetDepartmentByIdHandler : IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentResponse>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;

        public GetDepartmentByIdHandler(IDepartmentRepository departmentRepository, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        public async Task<Result<DepartmentResponse>> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
        {
            var department = await _departmentRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (department == null)
                return Result<DepartmentResponse>.NotFound($"Department with ID {request.Id} not found.");

            return Result<DepartmentResponse>.Success(_mapper.Map<DepartmentResponse>(department));
        }
    }
}
