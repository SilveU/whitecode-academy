using Application.Localization;
using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Enums;
using Domain.Entites.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Instructors.Commands.AssignInstructor
{
    public class AssignInstructorHandler : IRequestHandler<AssignInstructorCommand, Result<InstructorResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
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
            ILogger<AssignInstructorHandler> logger,
            ICacheService cache,
            IConfiguration configuration)
        {
            _instructorRepository = instructorRepository;
            _departmentRepository = departmentRepository;
            _auditLogRepository = auditLogRepository;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<Result<InstructorResponse>> Handle(AssignInstructorCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} was not found.", request.UserId);
                return Result<InstructorResponse>.NotFound(MessageKeys.Common.Instructor_UserNotFound);
            }

            var existingInstructor = await _instructorRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (existingInstructor != null)
            {
                _logger.LogWarning("User {UserId} is already assigned as an instructor.", request.UserId);
                return Result<InstructorResponse>.Failure(MessageKeys.Common.Instructor_AlreadyExists, 409);
            }

            if (request.DepartmentId.HasValue)
            {
                var department = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value, cancellationToken);
                if (department == null)
                {
                    _logger.LogWarning("Department {DepartmentId} was not found.", request.DepartmentId);
                    return Result<InstructorResponse>.NotFound(MessageKeys.Common.Department_NotFound);
                }
            }

            var instructor = new Instructor
            {
                UserId = request.UserId,
                DepartmentId = request.DepartmentId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _instructorRepository.CreateAsync(instructor, cancellationToken);
            await _userManager.AddToRoleAsync(user, Role.Instructor.ToString());
            await _instructorRepository.SaveChangesAsync(cancellationToken);

            var created = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(instructor.Id, cancellationToken);
            var response = _mapper.Map<InstructorResponse>(created!);

            await _cache.RemoveByPrefixAsync(CacheKeys.InstructorsPrefix(), cancellationToken);

            var redisKey = CacheKeys.Instructor(instructor.Id);
            await _cache.SetAsync<InstructorResponse>(redisKey, response,
                TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:InstructorExpirationMinutes")),
                cancellationToken);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = "system",
                Action = "Create",
                EntityName = nameof(Instructor),
                EntityId = instructor.Id,
                OldValues = null,
                NewValues = Serializer.Serialize(response),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            }, cancellationToken);

            _logger.LogInformation("Instructor profile created for user {UserId} with ID {InstructorId}.", request.UserId, instructor.Id);

            return Result<InstructorResponse>.Success(response, 201);
        }
    }
}
