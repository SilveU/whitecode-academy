using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Instructors.Commands.UpdateInstructor
{
    public record UpdateInstructorCommand : IRequest<Result<InstructorResponse>>
    {
        public Guid? Id { get; init; }
        public Guid? DepartmentId { get; set; }
    }
}
