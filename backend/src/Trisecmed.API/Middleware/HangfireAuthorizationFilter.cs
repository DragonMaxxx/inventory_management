using Hangfire.Dashboard;

namespace Trisecmed.API.Middleware;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // In development, allow access without auth
        if (httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
            return true;

        // In production, require Administrator role
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Administrator");
    }
}
