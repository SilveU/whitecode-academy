using API.Attributes;
using API.Controllers.Common;
using Application.DTOs.Core.Requests;
using Application.Features.Sections.Commands.CreateSection;
using Application.Features.Sections.Commands.DeleteSection;
using Application.Features.Sections.Commands.UpdateSection;
using Application.Features.Sections.Queries.GetSectionsByCourse;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers.Core
{
    [Route("api/[controller]")]
    [EnableRateLimiting("ReadPolicy")]
    public class SectionController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SectionController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper   = mapper;
        }

        // GET /api/section/by-course/{courseId}
        [HttpGet("by-course/{courseId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetSectionsByCourse(Guid courseId)
        {
            var result = await _mediator.Send(new GetSectionsByCourseQuery(courseId));
            if (result.IsSuccess)
                return Ok(result.Value);

            return Failure(result);
        }

        // POST /api/section
        // Multipart/form-data — VideoFile is required, PdfFile is optional
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [EnableRateLimiting("HeavyPolicy")]
        [Idempotent]
        public async Task<IActionResult> CreateSection([FromForm] CreateSectionRequest request)
        {
            var command = _mapper.Map<CreateSectionCommand>(request);
            command = command with
            {
                CurrentUserId = GetCurrentUserId(),
                IsInstructor  = User.IsInRole("Instructor")
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetSectionsByCourse), new { courseId = result.Value!.CourseId }, result.Value);

            return Failure(result);
        }

        // PUT /api/section/{id}
        // Multipart/form-data — all fields optional; only provided values are updated
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Instructor")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> UpdateSection(Guid id, [FromForm] UpdateSectionRequest request)
        {
            var command = _mapper.Map<UpdateSectionCommand>(request);
            command = command with
            {
                Id = id,
                CurrentUserId = GetCurrentUserId(),
                IsInstructor = User.IsInRole("Instructor")
            };

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
                return Ok(result.Value);

            return Failure(result);
        }

        // DELETE /api/section/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,Instructor")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> DeleteSection(Guid id)
        {
            var result = await _mediator.Send(
                new DeleteSectionCommand(id, GetCurrentUserId(), User.IsInRole("Instructor")));

            if (result.IsSuccess)
                return NoContent();

            return Failure(result);
        }
    }
}
