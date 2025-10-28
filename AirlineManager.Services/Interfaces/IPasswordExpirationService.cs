using AirlineManager.Models.Domain;

namespace AirlineManager.Services.Interfaces
{
    public interface IPasswordExpirationService
    {
        /// <summary>
        /// Checks if user's password has expired
        /// </summary>
        Task<bool> IsPasswordExpiredAsync(ApplicationUser user);

        /// <summary>
        /// Gets the number of days until password expires (negative if already expired)
        /// </summary>
        Task<int?> GetDaysUntilExpirationAsync(ApplicationUser user);

        /// <summary>
        /// Updates the password changed date for a user
        /// </summary>
        Task UpdatePasswordChangedDateAsync(ApplicationUser user);

        /// <summary>
        /// Gets the configured password expiration days from database
        /// </summary>
        Task<int> GetPasswordExpirationDaysAsync();
    }
}