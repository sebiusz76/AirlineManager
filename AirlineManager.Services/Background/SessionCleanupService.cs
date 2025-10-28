using AirlineManager.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineManager.Services.Background
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly ILogger<SessionCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(30);

        public SessionCleanupService(
          ILogger<SessionCleanupService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredSessionsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired sessions.");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Session Cleanup Service is stopping.");
        }

        private async Task CleanupExpiredSessionsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var sessionManagementService = scope.ServiceProvider.GetRequiredService<ISessionManagementService>();

            try
            {
                await sessionManagementService.CleanupExpiredSessionsAsync();
                _logger.LogInformation("Expired sessions cleaned up successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired sessions");
            }
        }
    }
}