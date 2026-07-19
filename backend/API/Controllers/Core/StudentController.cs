using API.Attributes;
using API.Controllers.Common;
using Application.Features.Students.Commands.AssignStudent;
using Application.Features.Students.Commands.DeleteStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Core
{
    [Route("api/[controller]")]
    public class StudentController : BaseController
    {
        private readonly IMediator _mediator;

        public StudentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/student
        // A logged-in user registers themselves as a student.
        // No body needed — the userId is taken from the JWT.
        [HttpPost]
        [Authorize(Roles = "User")]
        [EnableRateLimiting("HeavyPolicy")]
        [Idempotent]
        public async Task<IActionResult> AssignStudent()
        {
            var userId = GetCurrentUserId();

            var result = await _mediator.Send(new AssignStudentCommand(userId));
            if (result.IsSuccess)
                return CreatedAtAction(nameof(AssignStudent), result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // DELETE /api/student/{id}
        // Admin-only — soft-deletes the student profile and their enrollments
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            var result = await _mediator.Send(new DeleteStudentCommand(id));
            if (result.IsSuccess)
                return NoContent();

            return StatusCode(result.StatusCode, result);
        }
    }
}
