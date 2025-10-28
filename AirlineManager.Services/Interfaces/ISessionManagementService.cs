using AirlineManager.Models.Domain;

namespace AirlineManager.Services.Interfaces
{
    public interface ISessionManagementService
    {
        /// <summary>
        /// Creates or updates a session for the user
        /// </summary>
        Task<UserSession> CreateOrUpdateSessionAsync(string userId, string userEmail, string sessionId, string? ipAddress, string? userAgent, bool isPersistent);

        /// <summary>
        /// Gets all active sessions for a user
        /// </summary>
        Task<IEnumerable<UserSession>> GetActiveSessionsAsync(string userId);

        /// <summary>
        /// Updates the last activity time for a session
        /// </summary>
        Task UpdateSessionActivityAsync(string sessionId);

        /// <summary>
        /// Terminates a specific session
        /// </summary>
        Task TerminateSessionAsync(string sessionId);

        /// <summary>
        /// Terminates all sessions for a user except the current one
        /// </summary>
        Task TerminateOtherSessionsAsync(string userId, string currentSessionId);

        /// <summary>
        /// Terminates all sessions for a user
        /// </summary>
        Task TerminateAllSessionsAsync(string userId);

        /// <summary>
        /// Cleans up expired sessions
        /// </summary>
        Task CleanupExpiredSessionsAsync();

        /// <summary>
        /// Gets a session by session ID
        /// </summary>
        Task<UserSession?> GetSessionByIdAsync(string sessionId);
    }
}