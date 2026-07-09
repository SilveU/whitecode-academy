using Application.Common;
using Application.Features.Enrollments.Commands.CreateEnrollment;
using Application.Features.Enrollments.Commands.DeleteEnrollment;
using Application.Features.Enrollments.Queries.GetEnrollmentsByCourse;
using Application.Features.Enrollments.Queries.GetEnrollmentsByStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core
{
    [Route("api/[controller]")]
    public class EnrollmentController : BaseController
    {
        private readonly IMediator _mediator;

        public EnrollmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET /api/enrollment/by-course/{courseId}
        [HttpGet("by-course/{courseId:guid}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetEnrollmentsByCourse(Guid courseId)
        {
            var result = await _mediator.Send(new GetEnrollmentsByCourseQuery(courseId));
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // GET /api/enrollment/by-student/{studentId}
        [HttpGet("by-student/{studentId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetEnrollmentsByStudent(Guid studentId)
        {
            var result = await _mediator.Send(new GetEnrollmentsByStudentQuery(studentId));
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // POST /api/enrollment
        // Only CourseId comes from the body — StudentId is resolved from the JWT inside the handler
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Enroll([FromBody] EnrollBody body)
        {
            var command = new CreateEnrollmentCommand
            {
                CurrentUserId = GetCurrentUserId(),
                CourseId      = body.CourseId
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(Enroll), result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // DELETE /api/enrollment?studentId=&courseId=
        // Admin-only — can unenroll any student from any course
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unenroll([FromQuery] Guid studentId, [FromQuery] Guid courseId)
        {
            var result = await _mediator.Send(new DeleteEnrollmentCommand(studentId, courseId));
            if (result.IsSuccess)
                return NoContent();

            return StatusCode(result.StatusCode, result);
        }
    }

    /// <summary>
    /// Only the target CourseId is accepted from the caller.
    /// The student identity is always derived server-side from the JWT.
    /// </summary>
    public record EnrollBody(Guid CourseId);
}
