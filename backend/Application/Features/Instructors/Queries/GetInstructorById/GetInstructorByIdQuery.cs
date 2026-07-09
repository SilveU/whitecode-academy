using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Instructors.Queries.GetInstructorById
{
    public record GetInstructorByIdQuery(Guid Id) : IRequest<Result<InstructorResponse>>;
}
