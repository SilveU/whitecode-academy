using Application.Common;
using Application.DTOs.Core;
using MediatR;

namespace Application.Features.Courses.Queries.GetCourseById
{
    public record GetCourseByIdQuery(Guid Id) : IRequest<Result<CourseResponse>>;
}
