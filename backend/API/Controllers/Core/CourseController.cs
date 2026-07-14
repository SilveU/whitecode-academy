using Application.DTOs.Core.Requests;
using Application.Features.Courses.Commands.CreateCourse;
using Application.Features.Courses.Commands.DeleteCourse;
using Application.Features.Courses.Commands.UpdateCourse;
using Application.Features.Courses.Queries.GetCourseById;
using Application.Features.Courses.Queries.GetCourses;
using Application.Common;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Core
{
    [Route("api/[controller]")]
    [EnableRateLimiting("ReadPolicy")]
    public class CourseController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CourseController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper   = mapper;
        }

        // GET /api/course?pageNumber=1&pageSize=10&wordForSearch=...&sortBy=name
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCourses([FromQuery] QueryParameters parameters)
        {
            var result = await _mediator.Send(new GetCoursesQuery(parameters));
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // GET /api/course/{id}
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            var result = await _mediator.Send(new GetCourseByIdQuery(id));
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // POST /api/course
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        {
            var command = _mapper.Map<CreateCourseCommand>(request);
            command = command with
            {
                CurrentUserId = GetCurrentUserId(),
                IsInstructor  = User.IsInRole("Instructor")
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetCourseById), new { id = result.Value!.Id }, result.Value);

            return StatusCode(result.StatusCode, result);
        }

        // PUT /api/course/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Instructor")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseRequest request)
        {
            var command = _mapper.Map<UpdateCourseCommand>(request);
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
        [Authorize(Roles = "Admin,Instructor")]
        [EnableRateLimiting("HeavyPolicy")]
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
