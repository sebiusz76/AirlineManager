using AirlineManager.Models.Domain;
using AirlineManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AirlineManager.Middleware
{
    /// <summary>
    /// Middleware to handle maintenance mode - only SuperAdmin can access the site during maintenance
    /// </summary>
    public class MaintenanceModeMiddleware
    {
        private readonly RequestDelegate _next;

        public MaintenanceModeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
          IConfigurationService configService,
         UserManager<ApplicationUser> userManager)
        {
            // Check if maintenance mode is enabled
            var maintenanceEnabled = await configService.GetValueAsync<bool>("Maintenance_Mode_Enabled") ?? false;

            if (!maintenanceEnabled)
            {
                // Maintenance mode is disabled, continue with normal request
                await _next(context);
                return;
            }

            // Get the current path
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // Allow access to specific paths during maintenance (before checking authentication)
            if (path.StartsWith("/lib/") ||
                path.StartsWith("/css/") ||
              path.StartsWith("/js/") ||
                path.StartsWith("/images/") ||
          path.StartsWith("/favicon.ico") ||
                  path.StartsWith("/account/") ||  // Allow all Account actions (login, register, etc.)
        path.StartsWith("/maintenance"))
            {
                await _next(context);
                return;
            }

            // Maintenance mode is enabled - check if user is SuperAdmin
            var user = context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var appUser = await userManager.GetUserAsync(user);
                if (appUser != null)
                {
                    var roles = await userManager.GetRolesAsync(appUser);
                    if (roles.Contains("SuperAdmin"))
                    {
                        // SuperAdmin can access the site during maintenance
                        await _next(context);
                        return;
                    }
                }
            }

            // Redirect to maintenance page
            if (!path.StartsWith("/maintenance"))
            {
                context.Response.Redirect("/Maintenance");
                return;
            }

            await _next(context);
        }
    }
}