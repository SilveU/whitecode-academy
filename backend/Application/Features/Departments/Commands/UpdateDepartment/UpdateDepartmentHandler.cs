using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Departments.Commands.UpdateDepartment
{
    public class UpdateDepartmentHandler : IRequestHandler<UpdateDepartmentCommand, Result<DepartmentResponse>>
    {
        private readonly ILogger<UpdateDepartmentHandler> _logger;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public UpdateDepartmentHandler(
            IDepartmentRepository departmentRepository,
            IFileStorageService fileStorageService,
            IFileSecurityService fileSecurityService,
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ILogger<UpdateDepartmentHandler> logger)
        {
            _departmentRepository = departmentRepository;
            _fileStorageService   = fileStorageService;
            _fileSecurityService  = fileSecurityService;
            _auditLogRepository   = auditLogRepository;
            _mapper               = mapper;
            _logger               = logger;
        }

        public async Task<Result<DepartmentResponse>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
            {
                _logger.LogWarning("Update department failed: missing Id.");
                return Result<DepartmentResponse>.Failure("Id cannot be empty.", 400);
            }

            var department = await _departmentRepository.GetByIdAsync(request.Id.Value);
            if (department == null)
            {
                _logger.LogWarning("Department {DepartmentId} was not found.", request.Id);
                return Result<DepartmentResponse>.NotFound($"Department with ID {request.Id} not found.");
            }

            var oldValues = AuditSerializer.Serialize(_mapper.Map<DepartmentResponse>(department));

            if (!string.IsNullOrEmpty(request.Name))        department.Name        = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) department.Description = request.Description;

            if (request.ImageFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.ImageFile);
                await _fileSecurityService.ScanAsync(request.ImageFile);

                if (!string.IsNullOrEmpty(department.ImageUrl))
                    await _fileStorageService.DeleteAsync(department.ImageUrl);

                var imageFolder = Path.Combine("Departments", department.Id.ToString(), "Images");
                department.ImageUrl = await _fileStorageService.UploadAsync(request.ImageFile, imageFolder);
            }

            _departmentRepository.Update(department);
            await _departmentRepository.SaveChangesAsync();

            var response = _mapper.Map<DepartmentResponse>(department);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = "system",
                Action     = "Update",
                EntityName = nameof(Department),
                EntityId   = department.Id,
                OldValues  = oldValues,
                NewValues  = AuditSerializer.Serialize(response),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation("Department {DepartmentId} updated successfully.", department.Id);

            return Result<DepartmentResponse>.Success(response);
        }
    }
}
