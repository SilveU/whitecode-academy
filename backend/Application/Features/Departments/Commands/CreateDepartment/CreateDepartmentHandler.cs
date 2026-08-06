using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly ILogger<CreateDepartmentHandler> _logger;
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
            IMapper mapper,
            ILogger<CreateDepartmentHandler> logger,
            ICacheService cache,
            IConfiguration configuration)
        {
            _departmentRepository = departmentRepository;
            _fileStorageService = fileStorageService;
            _fileSecurityService = fileSecurityService;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<Result<DepartmentResponse>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = _mapper.Map<Department>(request);
            department.Id = Guid.NewGuid();
            department.CreatedAt = DateTimeOffset.UtcNow;

            if (request.ImageFile != null)
            {
                await _fileSecurityService.ValidateImageAsync(request.ImageFile, cancellationToken);
                await _fileSecurityService.ScanAsync(request.ImageFile, cancellationToken);
                var imageFolder = Path.Combine("Departments", department.Id.ToString(), "Images");
                department.ImageUrl = await _fileStorageService.UploadAsync(request.ImageFile, imageFolder);
            }

            await _departmentRepository.CreateAsync(department, cancellationToken);
            await _departmentRepository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<DepartmentResponse>(department);

            await _cache.RemoveByPrefixAsync(CacheKeys.DepartmentsPrefix(), cancellationToken);

            var redisKey = CacheKeys.Department(department.Id);
            await _cache.SetAsync<DepartmentResponse>(redisKey, response,
                TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:DepartmentExpirationMinutes")),
                cancellationToken);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = "system",
                Action = "Create",
                EntityName = nameof(Department),
                EntityId = department.Id,
                OldValues = null,
                NewValues = Serializer.Serialize(response),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            }, cancellationToken);

            _logger.LogInformation("Department {DepartmentId} '{Name}' created successfully.", department.Id, department.Name);

            return Result<DepartmentResponse>.Success(response, 201);
        }
    }
}
