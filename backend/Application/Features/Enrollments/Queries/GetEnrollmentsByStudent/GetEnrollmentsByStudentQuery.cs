using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Enrollments.Queries.GetEnrollmentsByStudent
{
    public record GetEnrollmentsByStudentQuery(Guid StudentId) : IRequest<Result<IEnumerable<EnrollmentResponse>>>;
}
