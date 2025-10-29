using AirlineManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AirlineManager.Middleware
{
    public class LockoutConfigurationMiddleware
    {
        private readonly RequestDelegate _next;

        public LockoutConfigurationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }
    }

    /// <summary>
    /// Service to update Identity Lockout options from database configuration
    /// </summary>
    public class LockoutOptionsUpdater : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LockoutOptionsUpdater> _logger;

        public LockoutOptionsUpdater(
            IServiceProvider serviceProvider,
            ILogger<LockoutOptionsUpdater> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var lockoutService = scope.ServiceProvider.GetRequiredService<IAccountLockoutService>();
                var identityOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>();

                var (maxAttempts, durationMinutes) = await lockoutService.GetLockoutConfigurationAsync();

                // Update the options
                identityOptions.Value.Lockout.MaxFailedAccessAttempts = maxAttempts;
                identityOptions.Value.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(durationMinutes);
                identityOptions.Value.Lockout.AllowedForNewUsers = maxAttempts > 0;

                _logger.LogInformation("Lockout configuration updated: MaxAttempts={MaxAttempts}, Duration={Duration} minutes",
                    maxAttempts, durationMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update lockout configuration from database");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}