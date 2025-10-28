using AirlineManager.DataAccess.Data;
using AirlineManager.Models.Domain;
using UAParser;

namespace AirlineManager.Services
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly ApplicationDbContext _context;

        public LoginHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogLoginAttemptAsync(string userId, string userEmail, bool isSuccessful,
    string? ipAddress, string? userAgent, bool requiredTwoFactor = false,
   string? failureReason = null)
        {
            var loginHistory = new UserLoginHistory
            {
                UserId = userId,
                UserEmail = userEmail,
                LoginTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason,
                RequiredTwoFactor = requiredTwoFactor
            };

            // Parse user agent to extract browser, OS, device info
            if (!string.IsNullOrEmpty(userAgent))
            {
                try
                {
                    var uaParser = Parser.GetDefault();
                    var clientInfo = uaParser.Parse(userAgent);

                    loginHistory.Browser = $"{clientInfo.UA.Family} {clientInfo.UA.Major}";
                    loginHistory.OperatingSystem = $"{clientInfo.OS.Family} {clientInfo.OS.Major}";
                    loginHistory.Device = clientInfo.Device.Family != "Other" ? clientInfo.Device.Family : null;
                }
                catch
                {
                    // If parsing fails, just skip it
                }
            }

            _context.UserLoginHistories.Add(loginHistory);
            await _context.SaveChangesAsync();
        }
    }
}