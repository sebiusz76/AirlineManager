using AirlineManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AirlineManager.Middleware
{
    public class SessionActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionActivityMiddleware> _logger;

        public SessionActivityMiddleware(RequestDelegate next, ILogger<SessionActivityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ISessionManagementService sessionManagementService)
        {
            // Update session activity if user is authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var sessionId = context.Session.Id;
                if (!string.IsNullOrEmpty(sessionId))
                {
                    try
                    {
                        await sessionManagementService.UpdateSessionActivityAsync(sessionId);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't interrupt request
                        _logger.LogError(ex, "Failed to update session activity for session {SessionId}", sessionId);
                    }
                }
            }

            await _next(context);
        }
    }
}