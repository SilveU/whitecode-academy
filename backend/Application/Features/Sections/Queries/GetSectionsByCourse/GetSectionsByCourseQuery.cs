using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Sections.Queries.GetSectionsByCourse
{
    public record GetSectionsByCourseQuery(Guid CourseId) : IRequest<Result<IEnumerable<SectionResponse>>>;
}
