namespace AirlineManager.Services.Interfaces
{
    public interface IAccountLockoutService
    {
        /// <summary>
        /// Gets the maximum allowed failed login attempts from configuration
        /// </summary>
        Task<int> GetMaxFailedLoginAttemptsAsync();

        /// <summary>
        /// Gets the lockout duration in minutes from configuration
        /// </summary>
        Task<int> GetLockoutDurationMinutesAsync();

        /// <summary>
        /// Checks if account lockout is enabled (MaxFailedLoginAttempts > 0)
        /// </summary>
        Task<bool> IsLockoutEnabledAsync();

        /// <summary>
        /// Applies lockout configuration to Identity options
        /// </summary>
        Task<(int MaxAttempts, int DurationMinutes)> GetLockoutConfigurationAsync();
    }
}