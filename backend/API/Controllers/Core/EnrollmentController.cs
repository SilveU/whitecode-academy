using Application.Features.Enrollments.Commands.CreateEnrollment;
using Application.Features.Enrollments.Commands.DeleteEnrollment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core
{
    [Authorize]
    [Route("api/[controller]")]
    public class EnrollmentController : BaseController
    {
        private readonly IMediator _mediator;

        public EnrollmentController(IMediator mediator)
        {
            _mediator = mediator;
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
