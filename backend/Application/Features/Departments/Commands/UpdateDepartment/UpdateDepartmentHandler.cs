using Application.Common;
using Application.DTOs.Core;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;

namespace Application.Features.Departments.Commands.UpdateDepartment
{
    public class UpdateDepartmentHandler : IRequestHandler<UpdateDepartmentCommand, Result<DepartmentResponse>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly IMapper _mapper;

        public UpdateDepartmentHandler(
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

            // Image file replacement
            if (request.ImageFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.ImageFile);   // reuses image extension + size validation
                await _fileSecurityService.ScanAsync(request.ImageFile);

                // Delete old image before uploading the new one
                if (!string.IsNullOrEmpty(department.ImageUrl))
                    await _fileStorageService.DeleteAsync(department.ImageUrl);

                var imageFolder = Path.Combine("Departments", department.Id.ToString(), "Images");
                department.ImageUrl = await _fileStorageService.UploadAsync(request.ImageFile, imageFolder);
            }

            _departmentRepository.Update(department);
            await _departmentRepository.SaveChangesAsync();

            return Result<DepartmentResponse>.Success(_mapper.Map<DepartmentResponse>(department));
        }
    }
}
