using Application.Common;
using Application.Helper;
using Application.Interfaces.Repositories;
using Domain.Entites.Audits;
using Domain.Entites.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Departments.Commands.DeleteDepartment
{
    public class DeleteDepartmentHandler : IRequestHandler<DeleteDepartmentCommand, Result<bool>>
    {
        private readonly ILogger<DeleteDepartmentHandler> _logger;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public DeleteDepartmentHandler(
            IDepartmentRepository departmentRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<DeleteDepartmentHandler> logger)
        {
            _departmentRepository = departmentRepository;
            _auditLogRepository   = auditLogRepository;
            _logger               = logger;
        }

        public async Task<Result<bool>> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _departmentRepository.GetByIdAsync(request.Id);
            if (department == null)
            {
                _logger.LogWarning("Department {DepartmentId} was not found.", request.Id);
                return Result<bool>.NotFound($"Department with ID {request.Id} not found.");
            }

            var hasActiveDependencies = await _departmentRepository.HasActiveCoursesOrInstructorsAsync(request.Id);
            if (hasActiveDependencies)
            {
                _logger.LogWarning(
                    "Department {DepartmentId} cannot be deleted because it has active courses or instructors.",
                    request.Id);
                return Result<bool>.Failure(
                    "Cannot delete this department because it has active courses or instructors assigned to it.", 409);
            }

            _departmentRepository.Delete(department);
            await _departmentRepository.SaveChangesAsync();

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = "system",
                Action = "Delete",
                EntityName = nameof(Department),
                EntityId = department.Id,
                OldValues = null,
                NewValues = null,
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation("Department {DepartmentId} '{Name}' deleted successfully.", department.Id, department.Name);

            return Result<bool>.Success(true);
        }
    }
}
