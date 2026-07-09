using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourses
{
    public record GetCoursesQuery(QueryParameters Parameters) : IRequest<Result<IEnumerable<CourseResponse>>>;
}
