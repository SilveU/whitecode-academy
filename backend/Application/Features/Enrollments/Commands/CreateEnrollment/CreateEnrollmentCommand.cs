using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Enrollments.Commands.CreateEnrollment
{
    public record CreateEnrollmentCommand : IRequest<Result<EnrollmentResponse>>
    {
        // Set by the controller from the JWT claim — never from the request body
        public string CurrentUserId { get; set; } = null!;
        public Guid CourseId { get; set; }
    }
}
