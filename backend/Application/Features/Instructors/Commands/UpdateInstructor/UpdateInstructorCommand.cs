using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Instructors.Commands.UpdateInstructor
{
    public record UpdateInstructorCommand : IRequest<Result<InstructorResponse>>
    {
        // Injected from the route
        public Guid? Id { get; init; }

        // Mapped from UpdateInstructorRequest via AutoMapper
        public Guid? DepartmentId { get; set; }
    }
}
