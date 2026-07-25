using Domain.Entites.Enums;
using Hangfire.Dashboard;

namespace API.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAsyncAuthorizationFilter
    {
        public Task<bool> AuthorizeAsync(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var authorized =
                httpContext.User.Identity?.IsAuthenticated == true &&
                httpContext.User.IsInRole(Role.Admin.ToString());

            return Task.FromResult(authorized);
        }
    }
}