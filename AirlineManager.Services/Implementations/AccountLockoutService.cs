using AirlineManager.Services.Interfaces;

namespace AirlineManager.Services.Implementations
{
    public class AccountLockoutService : IAccountLockoutService
    {
        private readonly IConfigurationService _configService;

        public AccountLockoutService(IConfigurationService configService)
        {
            _configService = configService;
        }

        public async Task<int> GetMaxFailedLoginAttemptsAsync()
        {
            try
            {
                var value = await _configService.GetValueAsync<int>("Security_MaxFailedLoginAttempts");
                return value ?? 5; // Default to 5 attempts
            }
            catch
            {
                return 5;
            }
        }

        public async Task<int> GetLockoutDurationMinutesAsync()
        {
            try
            {
                var value = await _configService.GetValueAsync<int>("Security_LockoutDurationMinutes");
                return value ?? 30; // Default to 30 minutes
            }
            catch
            {
                return 30;
            }
        }

        public async Task<bool> IsLockoutEnabledAsync()
        {
            var maxAttempts = await GetMaxFailedLoginAttemptsAsync();
            return maxAttempts > 0;
        }

        public async Task<(int MaxAttempts, int DurationMinutes)> GetLockoutConfigurationAsync()
        {
            var maxAttempts = await GetMaxFailedLoginAttemptsAsync();
            var durationMinutes = await GetLockoutDurationMinutesAsync();
            return (maxAttempts, durationMinutes);
        }
    }
}