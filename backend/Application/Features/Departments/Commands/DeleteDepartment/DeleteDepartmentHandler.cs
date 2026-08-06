using Application.Localization;
using Application.Common;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Departments.Commands.DeleteDepartment
{
    public class DeleteDepartmentHandler : IRequestHandler<DeleteDepartmentCommand, Result<bool>>
    {
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteDepartmentHandler> _logger;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public DeleteDepartmentHandler(
            IDepartmentRepository departmentRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<DeleteDepartmentHandler> logger,
            ICacheService cache)
        {
            _departmentRepository = departmentRepository;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Result<bool>> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _departmentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (department == null)
            {
                _logger.LogWarning("Department {DepartmentId} was not found.", request.Id);
                return Result<bool>.NotFound(MessageKeys.Common.Department_NotFound);
            }

            var hasActiveDependencies = await _departmentRepository.HasActiveCoursesOrInstructorsAsync(request.Id, cancellationToken);
            if (hasActiveDependencies)
            {
                _logger.LogWarning("Department {DepartmentId} cannot be deleted because it has active courses or instructors.", request.Id);
                return Result<bool>.Failure(MessageKeys.Common.Department_HasActiveDependencies, 409);
            }

            _departmentRepository.Delete(department);
            await _departmentRepository.SaveChangesAsync(cancellationToken);

            await _cache.RemoveAsync(CacheKeys.Department(department.Id), cancellationToken);
            await _cache.RemoveByPrefixAsync(CacheKeys.DepartmentsPrefix(), cancellationToken);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = "system",
                Action = "Delete",
                EntityName = nameof(Department),
                EntityId = department.Id,
                OldValues = null,
                NewValues = null,
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            }, cancellationToken);

            _logger.LogInformation("Department {DepartmentId} '{Name}' deleted successfully.", department.Id, department.Name);

            return Result<bool>.Success(true);
        }
    }
}
