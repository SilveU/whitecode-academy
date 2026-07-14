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

namespace Application.Features.Instructors.Commands.AssignInstructor
{
    public class AssignInstructorHandler : IRequestHandler<AssignInstructorCommand, Result<InstructorResponse>>
    {
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
            IMapper mapper)
        {
            _instructorRepository = instructorRepository;
            _departmentRepository = departmentRepository;
            _auditLogRepository = auditLogRepository;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<Result<InstructorResponse>> Handle(AssignInstructorCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Result<InstructorResponse>.NotFound($"User with ID {request.UserId} not found.");

            var existingInstructor = await _instructorRepository.GetByUserIdAsync(request.UserId);
            if (existingInstructor != null)
                return Result<InstructorResponse>.Failure("This user is already assigned as an instructor.", 409);

            if (request.DepartmentId.HasValue)
            {
                var department = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value);
                if (department == null)
                    return Result<InstructorResponse>.NotFound($"Department with ID {request.DepartmentId} not found.");
            }

            var instructor = new Instructor
            {
                UserId = request.UserId,
                DepartmentId = request.DepartmentId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _instructorRepository.CreateAsync(instructor);
            await _userManager.AddToRoleAsync(user, Role.Instructor.ToString());
            await _instructorRepository.SaveChangesAsync();

            var created = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(instructor.Id);
            var response = _mapper.Map<InstructorResponse>(created!);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = "system",       // action performed by Admin; no self-reference field on command
                Action = "Create",
                EntityName = nameof(Instructor),
                EntityId = instructor.Id,
                OldValues = null,
                NewValues = AuditSerializer.Serialize(response),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            });

            return Result<InstructorResponse>.Success(response, 201);
        }
    }
}
