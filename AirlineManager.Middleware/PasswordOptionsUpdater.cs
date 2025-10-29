using AirlineManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AirlineManager.Middleware
{
    /// <summary>
    /// Service to update Identity Password options from database configuration
    /// </summary>
    public class PasswordOptionsUpdater : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PasswordOptionsUpdater> _logger;

        public PasswordOptionsUpdater(
            IServiceProvider serviceProvider,
   ILogger<PasswordOptionsUpdater> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var passwordPolicyService = scope.ServiceProvider.GetRequiredService<IPasswordPolicyService>();
                var identityOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>();

                var policy = await passwordPolicyService.GetPasswordPolicyAsync();

                // Update the password options
                identityOptions.Value.Password.RequireDigit = policy.RequireDigit;
                identityOptions.Value.Password.RequireLowercase = policy.RequireLowercase;
                identityOptions.Value.Password.RequireUppercase = policy.RequireUppercase;
                identityOptions.Value.Password.RequireNonAlphanumeric = policy.RequireNonAlphanumeric;
                identityOptions.Value.Password.RequiredLength = policy.RequiredLength;
                identityOptions.Value.Password.RequiredUniqueChars = policy.RequiredUniqueChars;

                _logger.LogInformation("Password policy configuration updated: RequireDigit={RequireDigit}, RequireLowercase={RequireLowercase}, RequireUppercase={RequireUppercase}, RequireNonAlphanumeric={RequireNonAlphanumeric}, RequiredLength={RequiredLength}, RequiredUniqueChars={RequiredUniqueChars}",
       policy.RequireDigit, policy.RequireLowercase, policy.RequireUppercase, policy.RequireNonAlphanumeric, policy.RequiredLength, policy.RequiredUniqueChars);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update password policy configuration from database");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}