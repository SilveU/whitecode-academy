using Application.DTOs.Core.Requests;
using Application.Features.Instructors.Commands.AssignInstructor;
using Application.Features.Instructors.Commands.DeleteInstructor;
using Application.Features.Instructors.Commands.UpdateInstructor;
using Application.Features.Instructors.Queries.GetInstructorById;
using Application.Features.Instructors.Queries.GetInstructors;
using Application.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using API.Controllers.Common;

namespace API.Controllers.Core
{
    [Route("api/[controller]")]
    [EnableRateLimiting("ReadPolicy")]
    public class InstructorController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public InstructorController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper   = mapper;
        }

        // GET /api/instructor?pageNumber=1&pageSize=10&wordForSearch=...&sortBy=name
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInstructors([FromQuery] QueryParameters parameters)
        {
            var result = await _mediator.Send(new GetInstructorsQuery(parameters));
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // GET /api/instructor/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInstructorById(Guid id)
        {
            var result = await _mediator.Send(new GetInstructorByIdQuery(id));
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // POST /api/instructor
        // Admin assigns a registered user as an instructor.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> AssignInstructor([FromBody] AssignInstructorRequest request)
        {
            var command = _mapper.Map<AssignInstructorCommand>(request);

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetInstructorById), new { id = result.Value!.Id }, result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // PUT /api/instructor/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> UpdateInstructor(Guid id, [FromBody] UpdateInstructorRequest request)
        {
            var command = _mapper.Map<UpdateInstructorCommand>(request);
            command = command with { Id = id };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // DELETE /api/instructor/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> DeleteInstructor(Guid id)
        {
            var result = await _mediator.Send(new DeleteInstructorCommand(id));
            if (result.IsSuccess)
                return NoContent();

            return StatusCode(result.StatusCode, result);
        }
    }
}
