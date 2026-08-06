using Application.Localization;
using Application.Common;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entites.Audits;
using Domain.Entites.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Instructors.Commands.DeleteInstructor
{
    public class DeleteInstructorHandler : IRequestHandler<DeleteInstructorCommand, Result<bool>>
    {
        private readonly ICacheService _cache;
        private readonly ILogger<DeleteInstructorHandler> _logger;
        private readonly IInstructorRepository _instructorRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteInstructorHandler(
            IInstructorRepository instructorRepository,
            ICourseRepository courseRepository,
            IAuditLogRepository auditLogRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<DeleteInstructorHandler> logger,
            ICacheService cache)
        {
            _instructorRepository = instructorRepository;
            _courseRepository = courseRepository;
            _auditLogRepository = auditLogRepository;
            _userManager = userManager;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Result<bool>> Handle(DeleteInstructorCommand request, CancellationToken cancellationToken)
        {
            var instructor = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.Id, cancellationToken);
            if (instructor == null)
            {
                _logger.LogWarning("Instructor {InstructorId} was not found.", request.Id);
                return Result<bool>.NotFound(MessageKeys.Common.Instructor_NotFound);
            }

            var hasActiveCourses = instructor.Courses.Any(c => !c.IsDeleted);
            if (hasActiveCourses)
            {
                _logger.LogWarning("Instructor {InstructorId} cannot be deleted because they have active courses.", request.Id);
                return Result<bool>.Failure(MessageKeys.Common.Instructor_HasActiveCourses, 409);
            }

            _instructorRepository.Delete(instructor);

            var user = await _userManager.FindByIdAsync(instructor.UserId);
            if (user != null)
                await _userManager.RemoveFromRoleAsync(user, "Instructor");

            await _instructorRepository.SaveChangesAsync(cancellationToken);

            await _cache.RemoveAsync(CacheKeys.Instructor(instructor.Id), cancellationToken);
            await _cache.RemoveByPrefixAsync(CacheKeys.InstructorsPrefix(), cancellationToken);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = "system",
                Action = "Delete",
                EntityName = nameof(Instructor),
                EntityId = instructor.Id,
                OldValues = null,
                NewValues = null,
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            }, cancellationToken);

            _logger.LogInformation("Instructor {InstructorId} deleted successfully.", request.Id);

            return Result<bool>.Success(true);
        }
    }
}
