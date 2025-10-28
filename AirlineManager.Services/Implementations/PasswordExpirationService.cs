using AirlineManager.Models.Domain;
using AirlineManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AirlineManager.Services.Implementations
{
    public class PasswordExpirationService : IPasswordExpirationService
    {
        private readonly IConfigurationService _configService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PasswordExpirationService(
  IConfigurationService configService,
     UserManager<ApplicationUser> userManager)
        {
            _configService = configService;
            _userManager = userManager;
        }

        public async Task<bool> IsPasswordExpiredAsync(ApplicationUser user)
        {
            var expirationDays = await GetPasswordExpirationDaysAsync();

            // If expiration is disabled (0 days), password never expires
            if (expirationDays == 0)
            {
                return false;
            }

            // If PasswordChangedAt is null, consider it expired (force initial password change)
            if (!user.PasswordChangedAt.HasValue)
            {
                return true;
            }

            var daysSinceChange = (DateTime.UtcNow - user.PasswordChangedAt.Value).Days;
            return daysSinceChange >= expirationDays;
        }

        public async Task<int?> GetDaysUntilExpirationAsync(ApplicationUser user)
        {
            var expirationDays = await GetPasswordExpirationDaysAsync();

            // If expiration is disabled, return null
            if (expirationDays == 0)
            {
                return null;
            }

            // If PasswordChangedAt is null, password is expired
            if (!user.PasswordChangedAt.HasValue)
            {
                return 0;
            }

            var daysSinceChange = (DateTime.UtcNow - user.PasswordChangedAt.Value).Days;
            var daysRemaining = expirationDays - daysSinceChange;

            return daysRemaining;
        }

        public async Task UpdatePasswordChangedDateAsync(ApplicationUser user)
        {
            user.PasswordChangedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        public async Task<int> GetPasswordExpirationDaysAsync()
        {
            try
            {
                var value = await _configService.GetValueAsync<int>("Security_PasswordExpirationDays");
                return value ?? 90; // Default to 90 days if not configured
            }
            catch
            {
                // Fallback to 90 days if there's any error reading configuration
                return 90;
            }
        }
    }
}