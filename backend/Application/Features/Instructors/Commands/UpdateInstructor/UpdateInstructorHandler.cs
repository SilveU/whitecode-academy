using Application.Common;
using Application.DTOs.Core;
using Application.Helper;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entites.Audits;
using Domain.Entites.Users;
using MediatR;

namespace Application.Features.Instructors.Commands.UpdateInstructor
{
    public class UpdateInstructorHandler : IRequestHandler<UpdateInstructorCommand, Result<InstructorResponse>>
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public UpdateInstructorHandler(
            IInstructorRepository instructorRepository,
            IDepartmentRepository departmentRepository,
            IAuditLogRepository auditLogRepository,
            IMapper mapper)
        {
            _instructorRepository = instructorRepository;
            _departmentRepository = departmentRepository;
            _auditLogRepository = auditLogRepository;
            _mapper = mapper;
        }

        public async Task<Result<InstructorResponse>> Handle(UpdateInstructorCommand request, CancellationToken cancellationToken)
        {
            if (!request.Id.HasValue)
                return Result<InstructorResponse>.Failure("Id cannot be empty.", 400);

            var instructor = await _instructorRepository.GetByIdWithNavigationPropertiesAsync(request.Id.Value);
            if (instructor == null)
                return Result<InstructorResponse>.NotFound($"Instructor with ID {request.Id} not found.");

            var oldValues = AuditSerializer.Serialize(_mapper.Map<InstructorResponse>(instructor));

            if (request.DepartmentId.HasValue)
            {
                var department = await _departmentRepository.GetByIdAsync(request.DepartmentId.Value);
                if (department == null)
                    return Result<InstructorResponse>.NotFound($"Department with ID {request.DepartmentId} not found.");

                instructor.DepartmentId = request.DepartmentId;
            }

            _instructorRepository.Update(instructor);
            await _instructorRepository.SaveChangesAsync();

            var response = _mapper.Map<InstructorResponse>(instructor);

            await _auditLogRepository.LogAsync(new AuditLog
            {
                UserId = "system",
                Action = "Update",
                EntityName = nameof(Instructor),
                EntityId = instructor.Id,
                OldValues = oldValues,
                NewValues = AuditSerializer.Serialize(response),
                IpAddress = await IpAddressHelper.GetRealPublicIpAsync()
            });

            return Result<InstructorResponse>.Success(response);
        }
    }
}
