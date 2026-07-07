using Application.Features.Courses.Commands.CreateCourse;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CourseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);

            else
                return BadRequest(result);
        }
    }
}