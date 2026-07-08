using Application.Features.Courses.Commands.CreateCourse;
using Application.Features.Courses.Commands.DeleteCourse;
using Application.Features.Courses.Commands.UpdateCourse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core
{
    [Authorize(Roles = "Admin,Instructor")]
    [Route("api/[controller]")]
    public class CourseController : BaseController
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/course
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand command)
        {
            command = command with
            {
                CurrentUserId = GetCurrentUserId(),
                IsInstructor  = User.IsInRole("Instructor")
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(CreateCourse), result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // PUT /api/course/{id}  — id comes from the route, never from the body
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseCommand command)
        {
            command = command with
            {
                Id            = id,
                CurrentUserId = GetCurrentUserId(),
                IsInstructor  = User.IsInRole("Instructor")
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // DELETE /api/course/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var result = await _mediator.Send(
                new DeleteCourseCommand(id, GetCurrentUserId(), User.IsInRole("Instructor")));

            if (result.IsSuccess)
                return NoContent();

            return StatusCode(result.StatusCode, result);
        }
    }
}
