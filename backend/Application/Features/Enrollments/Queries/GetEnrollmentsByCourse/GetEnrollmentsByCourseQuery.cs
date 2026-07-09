using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Enrollments.Queries.GetEnrollmentsByCourse
{
    public record GetEnrollmentsByCourseQuery(Guid CourseId) : IRequest<Result<IEnumerable<EnrollmentResponse>>>;
}
