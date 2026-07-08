using Application.Features.Departments.Commands.CreateDepartment;
using Application.Features.Departments.Commands.DeleteDepartment;
using Application.Features.Departments.Commands.UpdateDepartment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class DepartmentController : BaseController
    {
        private readonly IMediator _mediator;

        public DepartmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/department
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(CreateDepartment), result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // PUT /api/department/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentCommand command)
        {
            command = command with { Id = id };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // DELETE /api/department/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var result = await _mediator.Send(new DeleteDepartmentCommand(id));
            if (result.IsSuccess)
                return NoContent();

            return StatusCode(result.StatusCode, result);
        }
    }
}
