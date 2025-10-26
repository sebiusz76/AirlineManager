using AirlineManager.Models.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AirlineManager.Middleware
{
    public class RequirePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public RequirePasswordChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userManager = context.RequestServices.GetService(typeof(UserManager<ApplicationUser>)) as UserManager<ApplicationUser>;
                if (userManager != null)
                {
                    var appUser = await userManager.GetUserAsync(user);
                    if (appUser != null && appUser.MustChangePassword)
                    {
                        var path = context.Request.Path.Value ?? string.Empty;
                        // Allow access to ChangePassword, Logout, AccessDenied, 2FA endpoints and static files
                        if (!path.StartsWith("/Account/ChangePassword", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/Account/Logout", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/Account/AccessDenied", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/Account/LoginWith2fa", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/Account/LoginWithRecovery", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
                            && !path.StartsWith("/favicon.ico", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.Redirect("/Account/ChangePassword");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}