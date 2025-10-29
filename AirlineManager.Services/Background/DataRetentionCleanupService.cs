using AirlineManager.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirlineManager.Services.Background
{
    /// <summary>
    /// Background service that runs data retention cleanup daily
    /// </summary>
    public class DataRetentionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataRetentionCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run once per day

        public DataRetentionCleanupService(
          IServiceProvider serviceProvider,
             ILogger<DataRetentionCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Retention Cleanup Service started");

            // Wait for initial delay (e.g., 1 minute after startup)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running scheduled data retention cleanup");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var retentionService = scope.ServiceProvider.GetRequiredService<IDataRetentionService>();
                        var result = await retentionService.CleanupAllAsync();

                        if (result.TotalDeleted > 0)
                        {
                            _logger.LogInformation(
                                 "Cleanup completed: {Total} records deleted in {Duration}ms",
                                result.TotalDeleted,
                                     result.Duration.TotalMilliseconds);
                        }
                        else
                        {
                            _logger.LogInformation("Cleanup completed: No records to delete");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during scheduled data retention cleanup");
                }

                // Wait for next execution (24 hours)
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Data Retention Cleanup Service stopped");
        }
    }
}