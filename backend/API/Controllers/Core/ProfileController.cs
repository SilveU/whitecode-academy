using API.Controllers.Common;
using Application.DTOs.Profile;
using Application.Interfaces.Profile;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : BaseController
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId =  GetCurrentUserId();
            var result = await _profileService.GetProfileAsync(userId);
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequset requset)
        {
            var userId =  GetCurrentUserId();
            var result = await _profileService.UpdateProfile(userId, requset);
            if (result.IsSuccess)
                return Ok(result.Value);

            return StatusCode(result.StatusCode, result);
        }
    }
}