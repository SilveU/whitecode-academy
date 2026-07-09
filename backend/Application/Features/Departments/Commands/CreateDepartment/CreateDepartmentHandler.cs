using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Core;
using MediatR;

namespace Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentResponse>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly IMapper _mapper;

        public CreateDepartmentHandler(
            IDepartmentRepository departmentRepository,
            IFileStorageService fileStorageService,
            IFileSecurityService fileSecurityService,
            IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _fileStorageService   = fileStorageService;
            _fileSecurityService  = fileSecurityService;
            _mapper               = mapper;
        }

        public async Task<Result<DepartmentResponse>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = _mapper.Map<Department>(request);
            department.Id        = Guid.NewGuid();
            department.CreatedAt = DateTimeOffset.UtcNow;

            if (request.ImageFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.ImageFile);   // reuses image extension + size validation
                await _fileSecurityService.ScanAsync(request.ImageFile);
                var imageFolder = Path.Combine("Departments", department.Id.ToString(), "Images");
                department.ImageUrl = await _fileStorageService.UploadAsync(request.ImageFile, imageFolder);
            }

            await _departmentRepository.CreateAsync(department);
            await _departmentRepository.SaveChangesAsync();

            return Result<DepartmentResponse>.Success(_mapper.Map<DepartmentResponse>(department), 201);
        }
    }
}
