using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Returns the authenticated user's Identity ID from the JWT NameIdentifier claim.
        /// Throws UnauthorizedAccessException if the claim is missing.
        /// </summary>
        protected string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("User identity claim not found.");

            return userId;
        }
    }
}
