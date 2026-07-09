using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Instructors.Queries.GetInstructors
{
    public record GetInstructorsQuery(QueryParameters Parameters) : IRequest<Result<IEnumerable<InstructorResponse>>>;
}
