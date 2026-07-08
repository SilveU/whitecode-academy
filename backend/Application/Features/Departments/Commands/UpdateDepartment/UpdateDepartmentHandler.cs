using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Departments.Commands.UpdateDepartment
{
    public class UpdateDepartmentHandler : IRequestHandler<UpdateDepartmentCommand, Result<DepartmentResponse>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;

        public UpdateDepartmentHandler(IDepartmentRepository departmentRepository, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        public async Task<Result<DepartmentResponse>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
                return Result<DepartmentResponse>.Failure("Id cannot be empty.", 400);

            var department = await _departmentRepository.GetByIdAsync(request.Id.Value);
            if (department == null)
                return Result<DepartmentResponse>.NotFound($"Department with ID {request.Id} not found.");

            if (!string.IsNullOrEmpty(request.Name))
                department.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Description))
                department.Description = request.Description;

            if (request.ImageUrl != null)
                department.ImageUrl = request.ImageUrl;

            _departmentRepository.Update(department);
            await _departmentRepository.SaveChangesAsync();

            return Result<DepartmentResponse>.Success(_mapper.Map<DepartmentResponse>(department));
        }
    }
}
