using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Core;
using MediatR;

namespace Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentResponse>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;

        public CreateDepartmentHandler(IDepartmentRepository departmentRepository, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        public async Task<Result<DepartmentResponse>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = _mapper.Map<Department>(request);
            department.CreatedAt = DateTimeOffset.UtcNow;

            await _departmentRepository.CreateAsync(department);
            await _departmentRepository.SaveChangesAsync();

            return Result<DepartmentResponse>.Success(_mapper.Map<DepartmentResponse>(department), 201);
        }
    }
}
