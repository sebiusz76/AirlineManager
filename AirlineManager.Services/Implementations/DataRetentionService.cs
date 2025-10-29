using AirlineManager.DataAccess.Data;
using AirlineManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AirlineManager.Services.Implementations
{
    public class DataRetentionService : IDataRetentionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationService _configService;
        private readonly ILogger<DataRetentionService> _logger;

        public DataRetentionService(
    ApplicationDbContext context,
         IConfigurationService configService,
            ILogger<DataRetentionService> logger)
        {
            _context = context;
            _configService = configService;
            _logger = logger;
        }

        public async Task<DataRetentionConfiguration> GetRetentionConfigurationAsync()
        {
            var applicationLogsDays = await _configService.GetValueAsync<int>("DataRetention_ApplicationLogs_Days") ?? 90;
            var loginHistoryDays = await _configService.GetValueAsync<int>("DataRetention_LoginHistory_Days") ?? 180;
            var auditLogsDays = await _configService.GetValueAsync<int>("DataRetention_AuditLogs_Days") ?? 365;
            var inactiveSessionsDays = await _configService.GetValueAsync<int>("DataRetention_InactiveSessions_Days") ?? 30;
            var enableAutoCleanup = await _configService.GetValueAsync<bool>("DataRetention_EnableAutoCleanup") ?? true;

            return new DataRetentionConfiguration
            {
                ApplicationLogsDays = applicationLogsDays,
                LoginHistoryDays = loginHistoryDays,
                AuditLogsDays = auditLogsDays,
                InactiveSessionsDays = inactiveSessionsDays,
                EnableAutoCleanup = enableAutoCleanup
            };
        }

        public async Task<int> CleanupApplicationLogsAsync()
        {
            var config = await GetRetentionConfigurationAsync();
            if (config.ApplicationLogsDays == 0)
            {
                _logger.LogInformation("Application logs retention is disabled (0 days = keep forever)");
                return 0;
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-config.ApplicationLogsDays);
            var logsToDelete = await _context.ApplicationLogs
              .Where(l => l.Timestamp < cutoffDate)
                    .ToListAsync();

            var count = logsToDelete.Count;
            if (count > 0)
            {
                _context.ApplicationLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} application logs older than {Days} days", count, config.ApplicationLogsDays);
            }

            return count;
        }

        public async Task<int> CleanupLoginHistoryAsync()
        {
            var config = await GetRetentionConfigurationAsync();
            if (config.LoginHistoryDays == 0)
            {
                _logger.LogInformation("Login history retention is disabled (0 days = keep forever)");
                return 0;
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-config.LoginHistoryDays);

            // Delete in batches to avoid timeout
            var batchSize = 1000;
            var totalDeleted = 0;

            while (true)
            {
                var batch = await _context.UserLoginHistories
              .Where(l => l.LoginTime < cutoffDate)
             .Take(batchSize)
                  .ToListAsync();

                if (!batch.Any())
                    break;

                _context.UserLoginHistories.RemoveRange(batch);
                await _context.SaveChangesAsync();
                totalDeleted += batch.Count;

                _logger.LogInformation("Deleted batch of {Count} login history records", batch.Count);
            }

            if (totalDeleted > 0)
            {
                _logger.LogInformation("Deleted total of {Count} login history records older than {Days} days", totalDeleted, config.LoginHistoryDays);
            }

            return totalDeleted;
        }

        public async Task<int> CleanupAuditLogsAsync()
        {
            var config = await GetRetentionConfigurationAsync();
            if (config.AuditLogsDays == 0)
            {
                _logger.LogInformation("Audit logs retention is disabled (0 days = keep forever)");
                return 0;
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-config.AuditLogsDays);

            // Delete in batches
            var batchSize = 1000;
            var totalDeleted = 0;

            while (true)
            {
                var batch = await _context.UserAuditLogs
                          .Where(l => l.ModifiedAt < cutoffDate)
                      .Take(batchSize)
                    .ToListAsync();

                if (!batch.Any())
                    break;

                _context.UserAuditLogs.RemoveRange(batch);
                await _context.SaveChangesAsync();
                totalDeleted += batch.Count;

                _logger.LogInformation("Deleted batch of {Count} audit log records", batch.Count);
            }

            if (totalDeleted > 0)
            {
                _logger.LogInformation("Deleted total of {Count} audit logs older than {Days} days", totalDeleted, config.AuditLogsDays);
            }

            return totalDeleted;
        }

        public async Task<int> CleanupInactiveSessionsAsync()
        {
            var config = await GetRetentionConfigurationAsync();
            if (config.InactiveSessionsDays == 0)
            {
                _logger.LogInformation("Inactive sessions retention is disabled (0 days = keep forever)");
                return 0;
            }

            var cutoffDate = DateTime.UtcNow.AddDays(-config.InactiveSessionsDays);
            var sessionsToDelete = await _context.UserSessions
        .Where(s => !s.IsActive && s.LastActivityAt < cutoffDate)
         .ToListAsync();

            var count = sessionsToDelete.Count;
            if (count > 0)
            {
                _context.UserSessions.RemoveRange(sessionsToDelete);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} inactive sessions older than {Days} days", count, config.InactiveSessionsDays);
            }

            return count;
        }

        public async Task<DataRetentionResult> CleanupAllAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting data retention cleanup");

            var result = new DataRetentionResult
            {
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                var config = await GetRetentionConfigurationAsync();

                if (!config.EnableAutoCleanup)
                {
                    _logger.LogInformation("Auto cleanup is disabled in configuration");
                    stopwatch.Stop();
                    result.Duration = stopwatch.Elapsed;
                    return result;
                }

                result.ApplicationLogsDeleted = await CleanupApplicationLogsAsync();
                result.LoginHistoryDeleted = await CleanupLoginHistoryAsync();
                result.AuditLogsDeleted = await CleanupAuditLogsAsync();
                result.InactiveSessionsDeleted = await CleanupInactiveSessionsAsync();

                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;

                _logger.LogInformation(
             "Data retention cleanup completed in {Duration}ms. Total deleted: {Total} records (Logs: {Logs}, LoginHistory: {LoginHistory}, AuditLogs: {AuditLogs}, Sessions: {Sessions})",
              stopwatch.ElapsedMilliseconds,
            result.TotalDeleted,
              result.ApplicationLogsDeleted,
                 result.LoginHistoryDeleted,
                  result.AuditLogsDeleted,
              result.InactiveSessionsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data retention cleanup");
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                throw;
            }

            return result;
        }

        public async Task<DataRetentionStatistics> GetRetentionStatisticsAsync()
        {
            var config = await GetRetentionConfigurationAsync();
            var stats = new DataRetentionStatistics();

            // Application Logs
            stats.ApplicationLogsTotal = await _context.ApplicationLogs.CountAsync();
            stats.OldestApplicationLog = await _context.ApplicationLogs
              .OrderBy(l => l.Timestamp)
                    .Select(l => (DateTime?)l.Timestamp)
                   .FirstOrDefaultAsync();

            if (config.ApplicationLogsDays > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-config.ApplicationLogsDays);
                stats.ApplicationLogsToDelete = await _context.ApplicationLogs
           .Where(l => l.Timestamp < cutoffDate)
               .CountAsync();
            }

            // Login History
            stats.LoginHistoryTotal = await _context.UserLoginHistories.CountAsync();
            stats.OldestLoginHistory = await _context.UserLoginHistories
                     .OrderBy(l => l.LoginTime)
               .Select(l => (DateTime?)l.LoginTime)
                   .FirstOrDefaultAsync();

            if (config.LoginHistoryDays > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-config.LoginHistoryDays);
                stats.LoginHistoryToDelete = await _context.UserLoginHistories
                       .Where(l => l.LoginTime < cutoffDate)
                       .CountAsync();
            }

            // Audit Logs
            stats.AuditLogsTotal = await _context.UserAuditLogs.CountAsync();
            stats.OldestAuditLog = await _context.UserAuditLogs
                   .OrderBy(l => l.ModifiedAt)
            .Select(l => (DateTime?)l.ModifiedAt)
                      .FirstOrDefaultAsync();

            if (config.AuditLogsDays > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-config.AuditLogsDays);
                stats.AuditLogsToDelete = await _context.UserAuditLogs
   .Where(l => l.ModifiedAt < cutoffDate)
             .CountAsync();
            }

            // Inactive Sessions
            stats.InactiveSessionsTotal = await _context.UserSessions
              .Where(s => !s.IsActive)
                  .CountAsync();
            stats.OldestInactiveSession = await _context.UserSessions
           .Where(s => !s.IsActive)
          .OrderBy(s => s.LastActivityAt)
              .Select(s => (DateTime?)s.LastActivityAt)
          .FirstOrDefaultAsync();

            if (config.InactiveSessionsDays > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-config.InactiveSessionsDays);
                stats.InactiveSessionsToDelete = await _context.UserSessions
              .Where(s => !s.IsActive && s.LastActivityAt < cutoffDate)
        .CountAsync();
            }

            return stats;
        }
    }
}