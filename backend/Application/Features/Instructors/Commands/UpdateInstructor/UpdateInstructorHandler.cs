using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Users;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Instructors.Commands.UpdateInstructor
{
    public class UpdateInstructorHandler : IRequestHandler<UpdateInstructorCommand, Result<InstructorResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly ILogger<UpdateInstructorHandler> _logger;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public UpdateInstructorHandler(
            IInstructorRepository instructorRepository,
            IDepartmentRepository departmentRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper,
            ILogger<UpdateInstructorHandler> logger,
            ICacheService cache,
            IConfiguration configuration)
        {
            _instructorRepository = instructorRepository;
            _departmentRepository = departmentRepository;
            _auditLogRepository   = auditLogRepository;
            _mapper               = mapper;
            _logger               = logger;
            _cache                = cache;
            _configuration        = configuration;
        }

        public async Task<Result<InstructorResponse>> Handle(UpdateInstructorCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
            {
                _logger.LogWarning("Update instructor failed: missing Id.");
                return Result<InstructorResponse>.Failure("Id cannot be empty.", 400);
            }

            var instructor = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value);
            if (instructor == null)
            {
                _logger.LogWarning("Instructor {InstructorId} was not found.", request.Id);
                return Result<InstructorResponse>.NotFound($"Instructor with ID {request.Id} not found.");
            }

            var oldValues = Serializer.Serialize(_mapper.Map<InstructorResponse>(instructor));

            if (request.DepartmentId.HasValue)
            {
                var department = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value);
                if (department == null)
                {
                    _logger.LogWarning("Department {DepartmentId} was not found.", request.DepartmentId);
                    return Result<InstructorResponse>.NotFound($"Department with ID {request.DepartmentId} not found.");
                }

                instructor.DepartmentId = request.DepartmentId;
            }

            _instructorRepository.Update(instructor);
            await _instructorRepository.SaveChangesAsync();

            var response = _mapper.Map<InstructorResponse>(instructor);

            await _cache.RemoveAsync(CacheKeys.Instructor(instructor.Id));
            await _cache.RemoveByPrefixAsync(CacheKeys.InstructorsPrefix());

            var redisKey = CacheKeys.Instructor(instructor.Id);
            await _cache.SetAsync<InstructorResponse>(redisKey, response,
                TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:InstructorExpirationMinutes")));

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = "system",
                Action     = "Update",
                EntityName = nameof(Instructor),
                EntityId   = instructor.Id,
                OldValues  = oldValues,
                NewValues  = Serializer.Serialize(response),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation("Instructor {InstructorId} updated successfully.", instructor.Id);

            return Result<InstructorResponse>.Success(response);
        }
    }
}
