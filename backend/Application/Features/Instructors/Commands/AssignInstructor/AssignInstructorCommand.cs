using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Instructors.Commands.AssignInstructor
{
    public record AssignInstructorCommand : IRequest<Result<InstructorResponse>>
    {
        // Mapped from AssignInstructorRequest via AutoMapper
        public string UserId { get; set; } = null!;
        public Guid? DepartmentId { get; set; }
    }
}
