using Application.Common;
using Application.Helper;
using Application.Interfaces.Repositories;
using Domain.Entites.Audits;
using Domain.Entites.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentHandler : IRequestHandler<DeleteStudentCommand, Result<bool>>
    {
        private readonly ILogger<DeleteStudentHandler> _logger;
        private readonly IStudentRepository _studentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteStudentHandler(
            IStudentRepository studentRepository,
            IEnrollmentRepository enrollmentRepository,
            IAuditLogRepository auditLogRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<DeleteStudentHandler> logger)
        {
            _studentRepository    = studentRepository;
            _enrollmentRepository = enrollmentRepository;
            _auditLogRepository   = auditLogRepository;
            _userManager          = userManager;
            _logger               = logger;
        }

        public async Task<Result<bool>> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            var student = await _studentRepository.GetByIdWithNavigationPropertiesAsync(request.Id);
            if (student == null)
            {
                _logger.LogWarning("Student {StudentId} was not found.", request.Id);
                return Result<bool>.NotFound("Student not found.");
            }

            var enrollments = await _enrollmentRepository.GetByStudentIdAsync(request.Id);
            foreach (var enrollment in enrollments)
                _enrollmentRepository.Delete(enrollment);

            _studentRepository.Delete(student);
            await _studentRepository.SaveChangesAsync();

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = "system",
                Action     = "Delete",
                EntityName = nameof(Student),
                EntityId   = student.Id,
                OldValues  = null,
                NewValues  = null,
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation("Student {StudentId} and their enrollments deleted successfully.", request.Id);

            return Result<bool>.Success(true);
        }
    }
}
