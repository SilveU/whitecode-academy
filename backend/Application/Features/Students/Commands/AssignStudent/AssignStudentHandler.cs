using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Features.Students.Commands.AssignStudent
{
    public class AssignStudentHandler : IRequestHandler<AssignStudentCommand, Result<StudentResponse>>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly ILogger<AssignStudentHandler> _logger;
        private readonly IStudentRepository _studentRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AssignStudentHandler(
            IStudentRepository studentRepository,
            IAuditLogRepository auditLogRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<AssignStudentHandler> logger,
            ICacheService cache,
            IConfiguration configuration)
        {
            _studentRepository  = studentRepository;
            _auditLogRepository = auditLogRepository;
            _userManager        = userManager;
            _mapper             = mapper;
            _logger             = logger;
            _cache              = cache;
            _configuration      = configuration;
        }

        public async Task<Result<StudentResponse>> Handle(AssignStudentCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} was not found.", request.UserId);
                return Result<StudentResponse>.NotFound("User not found.");
            }

            var existingStudent = await _studentRepository.GetByUserIdAsync(request.UserId);
            if (existingStudent != null)
            {
                _logger.LogWarning("User {UserId} is already registered as a student.", request.UserId);
                return Result<StudentResponse>.Failure("This user is already registered as a student.", 409);
            }

            var student = new Student
            {
                UserId    = request.UserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _studentRepository.CreateAsync(student);
            await _studentRepository.SaveChangesAsync();

            var created  = await _studentRepository.GetByIdWithNavigationPropertiesAsync(student.Id);
            var response = _mapper.Map<StudentResponse>(created!);

            await _cache.RemoveByPrefixAsync(CacheKeys.StudentsPrefix());

            var redisKey = CacheKeys.Student(student.Id);
            await _cache.SetAsync<StudentResponse>(redisKey, response,
                TimeSpan.FromMinutes(_configuration.GetValue<double>("Redis:StudentExpirationMinutes")));

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId     = request.UserId,
                Action     = "Create",
                EntityName = nameof(Student),
                EntityId   = student.Id,
                OldValues  = null,
                NewValues  = Serializer.Serialize(response),
                IpAddress  = await IpAddressHelper.GetRealPublicIpAsync()
            });

            _logger.LogInformation("Student profile created for user {UserId} with ID {StudentId}.", request.UserId, student.Id);

            return Result<StudentResponse>.Success(response, 201);
        }
    }
}
