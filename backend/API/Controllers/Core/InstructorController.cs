using Application.Features.Instructors.Commands.AssignInstructor;
using Application.Features.Instructors.Commands.DeleteInstructor;
using Application.Features.Instructors.Commands.UpdateInstructor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class InstructorController : BaseController
    {
        private readonly IMediator _mediator;

        public InstructorController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/instructor
        // Admin assigns a registered user as an instructor.
        // UserId is a body param here — the Admin is designating someone else, not themselves.
        [HttpPost]
        public async Task<IActionResult> AssignInstructor([FromBody] AssignInstructorCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(AssignInstructor), result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // PUT /api/instructor/{id}
        // id (instructor profile ID) comes from the route
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateInstructor(Guid id, [FromBody] UpdateInstructorCommand command)
        {
            command = command with { Id = id };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // DELETE /api/instructor/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteInstructor(Guid id)
        {
            var result = await _mediator.Send(new DeleteInstructorCommand(id));
            if (result.IsSuccess)
                return NoContent();

            return StatusCode(result.StatusCode, result);
        }
    }
}
