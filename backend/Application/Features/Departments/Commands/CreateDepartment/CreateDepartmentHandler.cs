using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;

namespace Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentResponse>>
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public CreateDepartmentHandler(
            IDepartmentRepository departmentRepository,
            IFileStorageService fileStorageService,
            IFileSecurityService fileSecurityService,
            IAuditLogRepository auditLogRepository,
            IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _fileStorageService   = fileStorageService;
            _fileSecurityService  = fileSecurityService;
            _auditLogRepository   = auditLogRepository;
            _mapper               = mapper;
        }

        public async Task<Result<DepartmentResponse>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = _mapper.Map<Department>(request);
            department.Id        = Guid.NewGuid();
            department.CreatedAt = DateTimeOffset.UtcNow;

            if (request.ImageFile != null)
            {
                await _fileSecurityService.ValidatePdfAsync(request.ImageFile);
                await _fileSecurityService.ScanAsync(request.ImageFile);
                var imageFolder = Path.Combine("Departments", department.Id.ToString(), "Images");
                department.ImageUrl = await _fileStorageService.UploadAsync(request.ImageFile, imageFolder);
            }

            await _departmentRepository.CreateAsync(department);
            await _departmentRepository.SaveChangesAsync();

            var response = _mapper.Map<DepartmentResponse>(department);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = "system",      // Department creation is Admin-only; no ownership field in command — use "system" sentinel
                Action     = "Create",
                EntityName = nameof(Department),
                EntityId   = department.Id,
                OldValues  = null,
                NewValues  = AuditSerializer.Serialize(response),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            return Result<DepartmentResponse>.Success(response, 201);
        }
    }
}
