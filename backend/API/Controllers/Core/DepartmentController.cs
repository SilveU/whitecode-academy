using API.Controllers.Common;
using Application.Common;
using Application.DTOs.Core.Requests;
using Application.Features.Departments.Commands.CreateDepartment;
using Application.Features.Departments.Commands.DeleteDepartment;
using Application.Features.Departments.Commands.UpdateDepartment;
using Application.Features.Departments.Queries.GetDepartmentById;
using Application.Features.Departments.Queries.GetDepartments;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Core
{
    [Route("api/[controller]")]
    [EnableRateLimiting("ReadPolicy")]
    public class DepartmentController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public DepartmentController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper   = mapper;
        }

        // GET /api/department?pageNumber=1&pageSize=10&wordForSearch=...&sortBy=name
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetDepartments([FromQuery] QueryParameters parameters)
        {
            var result = await _mediator.Send(new GetDepartmentsQuery(parameters));
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // GET /api/department/{id}
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetDepartmentById(Guid id)
        {
            var result = await _mediator.Send(new GetDepartmentByIdQuery(id));
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // POST /api/department
        // Multipart/form-data — ImageFile is optional
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> CreateDepartment([FromForm] CreateDepartmentRequest request)
        {
            var command = _mapper.Map<CreateDepartmentCommand>(request);

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetDepartmentById), new { id = result.Value!.Id }, result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // PUT /api/department/{id}
        // Multipart/form-data — all fields optional
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> UpdateDepartment(Guid id, [FromForm] UpdateDepartmentRequest request)
        {
            var command = _mapper.Map<UpdateDepartmentCommand>(request);
            command = command with { Id = id };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // DELETE /api/department/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var result = await _mediator.Send(new DeleteDepartmentCommand(id));
            if (result.IsSuccess)
                return NoContent();

            return StatusCode(result.StatusCode, result);
        }
    }
}
