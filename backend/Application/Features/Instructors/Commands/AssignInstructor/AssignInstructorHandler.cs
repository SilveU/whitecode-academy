using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Enums;
using Domain.Entites.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Features.Instructors.Commands.AssignInstructor
{
    public class AssignInstructorHandler : IRequestHandler<AssignInstructorCommand, Result<InstructorResponse>>
    {
        private readonly ILogger<AssignInstructorHandler> _logger;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AssignInstructorHandler(
            IInstructorRepository instructorRepository,
            IDepartmentRepository departmentRepository,
            IAuditLogRepository auditLogRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<AssignInstructorHandler> logger)
        {
            _instructorRepository = instructorRepository;
            _departmentRepository = departmentRepository;
            _auditLogRepository   = auditLogRepository;
            _userManager          = userManager;
            _mapper               = mapper;
            _logger               = logger;
        }

        public async Task<Result<InstructorResponse>> Handle(AssignInstructorCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} was not found.", request.UserId);
                return Result<InstructorResponse>.NotFound($"User with ID {request.UserId} not found.");
            }

            var existingInstructor = await _instructorRepository.GetByUserIdAsync(request.UserId);
            if (existingInstructor != null)
            {
                _logger.LogWarning("User {UserId} is already assigned as an instructor.", request.UserId);
                return Result<InstructorResponse>.Failure("This user is already assigned as an instructor.", 409);
            }

            if (request.DepartmentId.HasValue)
            {
                var department = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value);
                if (department == null)
                {
                    _logger.LogWarning("Department {DepartmentId} was not found.", request.DepartmentId);
                    return Result<InstructorResponse>.NotFound($"Department with ID {request.DepartmentId} not found.");
                }
            }

            var instructor = new Instructor
            {
                UserId       = request.UserId,
                DepartmentId = request.DepartmentId,
                CreatedAt    = DateTimeOffset.UtcNow
            };

            await _instructorRepository.CreateAsync(instructor);
            await _userManager.AddToRoleAsync(user, Role.Instructor.ToString());
            await _instructorRepository.SaveChangesAsync();

            var created = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(instructor.Id);
            var response = _mapper.Map<InstructorResponse>(created!);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = "system",
                Action     = "Create",
                EntityName = nameof(Instructor),
                EntityId   = instructor.Id,
                OldValues  = null,
                NewValues  = AuditSerializer.Serialize(response),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation("Instructor profile created for user {UserId} with ID {InstructorId}.", request.UserId, instructor.Id);

            return Result<InstructorResponse>.Success(response, 201);
        }
    }
}
