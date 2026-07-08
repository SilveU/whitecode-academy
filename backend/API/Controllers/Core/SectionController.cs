using Application.Features.Sections.Commands.CreateSection;
using Application.Features.Sections.Commands.DeleteSection;
using Application.Features.Sections.Commands.UpdateSection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core
{
    [Authorize(Roles = "Admin,Instructor")]
    [Route("api/[controller]")]
    public class SectionController : BaseController
    {
        private readonly IMediator _mediator;

        public SectionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/section
        // CourseId is part of the body — it's input data, not a resource identifier
        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] CreateSectionCommand command)
        {
            command = command with
            {
                CurrentUserId = GetCurrentUserId(),
                IsInstructor  = User.IsInRole("Instructor")
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(CreateSection), result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // PUT /api/section/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSection(Guid id, [FromBody] UpdateSectionCommand command)
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

        // DELETE /api/section/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSection(Guid id)
        {
            var result = await _mediator.Send(
                new DeleteSectionCommand(id, GetCurrentUserId(), User.IsInRole("Instructor")));

            if (result.IsSuccess)
                return NoContent();

            return StatusCode(result.StatusCode, result);
        }
    }
}
