namespace AirlineManager.Services.Interfaces
{
    public interface ILoginHistoryService
    {
        Task LogLoginAttemptAsync(string userId, string userEmail, bool isSuccessful,
         string? ipAddress, string? userAgent, bool requiredTwoFactor = false,
             string? failureReason = null);
    }
}