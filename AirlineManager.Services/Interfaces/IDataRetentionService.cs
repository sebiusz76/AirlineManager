namespace AirlineManager.Services.Interfaces
{
    public interface IDataRetentionService
    {
        /// <summary>
        /// Gets data retention configuration from database
        /// </summary>
        Task<DataRetentionConfiguration> GetRetentionConfigurationAsync();

        /// <summary>
        /// Cleans up application logs older than retention period
        /// </summary>
        Task<int> CleanupApplicationLogsAsync();

        /// <summary>
        /// Cleans up login history older than retention period
        /// </summary>
        Task<int> CleanupLoginHistoryAsync();

        /// <summary>
        /// Cleans up audit logs older than retention period
        /// </summary>
        Task<int> CleanupAuditLogsAsync();

        /// <summary>
        /// Cleans up inactive sessions older than retention period
        /// </summary>
        Task<int> CleanupInactiveSessionsAsync();

        /// <summary>
        /// Runs all cleanup tasks based on retention policies
        /// </summary>
        Task<DataRetentionResult> CleanupAllAsync();

        /// <summary>
        /// Gets statistics about data that would be deleted
        /// </summary>
        Task<DataRetentionStatistics> GetRetentionStatisticsAsync();
    }

    public class DataRetentionConfiguration
    {
        public int ApplicationLogsDays { get; set; }
        public int LoginHistoryDays { get; set; }
        public int AuditLogsDays { get; set; }
        public int InactiveSessionsDays { get; set; }
        public bool EnableAutoCleanup { get; set; }
    }

    public class DataRetentionResult
    {
        public int ApplicationLogsDeleted { get; set; }
        public int LoginHistoryDeleted { get; set; }
        public int AuditLogsDeleted { get; set; }
        public int InactiveSessionsDeleted { get; set; }
        public int TotalDeleted => ApplicationLogsDeleted + LoginHistoryDeleted + AuditLogsDeleted + InactiveSessionsDeleted;
        public DateTime ExecutedAt { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class DataRetentionStatistics
    {
        public int ApplicationLogsToDelete { get; set; }
        public int ApplicationLogsTotal { get; set; }
        public DateTime? OldestApplicationLog { get; set; }

        public int LoginHistoryToDelete { get; set; }
        public int LoginHistoryTotal { get; set; }
        public DateTime? OldestLoginHistory { get; set; }

        public int AuditLogsToDelete { get; set; }
        public int AuditLogsTotal { get; set; }
        public DateTime? OldestAuditLog { get; set; }

        public int InactiveSessionsToDelete { get; set; }
        public int InactiveSessionsTotal { get; set; }
        public DateTime? OldestInactiveSession { get; set; }

        public int TotalToDelete => ApplicationLogsToDelete + LoginHistoryToDelete + AuditLogsToDelete + InactiveSessionsToDelete;
        public int TotalRecords => ApplicationLogsTotal + LoginHistoryTotal + AuditLogsTotal + InactiveSessionsTotal;
    }
}