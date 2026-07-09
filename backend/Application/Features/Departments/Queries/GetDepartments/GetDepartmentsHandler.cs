using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Departments.Queries.GetDepartments
{
    public class GetDepartmentsHandler : IRequestHandler<GetDepartmentsQuery, Result<IEnumerable<DepartmentResponse>>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;

        public GetDepartmentsHandler(IDepartmentRepository departmentRepository, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<DepartmentResponse>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var departments = await _departmentRepository.SearchAsync(request.Parameters);
            return Result<IEnumerable<DepartmentResponse>>.Success(_mapper.Map<IEnumerable<DepartmentResponse>>(departments));
        }
    }
}
