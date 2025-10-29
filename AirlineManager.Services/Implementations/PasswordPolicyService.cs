using AirlineManager.Services.Interfaces;

namespace AirlineManager.Services.Implementations
{
    public class PasswordPolicyService : IPasswordPolicyService
    {
        private readonly IConfigurationService _configService;

        public PasswordPolicyService(IConfigurationService configService)
        {
            _configService = configService;
        }

        public async Task<PasswordPolicyConfiguration> GetPasswordPolicyAsync()
        {
            var requireDigit = await _configService.GetValueAsync<bool>("Security_Password_RequireDigit") ?? true;
            var requireLowercase = await _configService.GetValueAsync<bool>("Security_Password_RequireLowercase") ?? true;
            var requireUppercase = await _configService.GetValueAsync<bool>("Security_Password_RequireUppercase") ?? true;
            var requireNonAlphanumeric = await _configService.GetValueAsync<bool>("Security_Password_RequireNonAlphanumeric") ?? false;
            var requiredLength = await _configService.GetValueAsync<int>("Security_Password_RequiredLength") ?? 8;
            var requiredUniqueChars = await _configService.GetValueAsync<int>("Security_Password_RequiredUniqueChars") ?? 1;

            return new PasswordPolicyConfiguration
            {
                RequireDigit = requireDigit,
                RequireLowercase = requireLowercase,
                RequireUppercase = requireUppercase,
                RequireNonAlphanumeric = requireNonAlphanumeric,
                RequiredLength = requiredLength,
                RequiredUniqueChars = requiredUniqueChars
            };
        }

        public async Task<List<string>> GetPasswordRequirementsAsync()
        {
            var policy = await GetPasswordPolicyAsync();
            var requirements = new List<string>();

            requirements.Add($"At least {policy.RequiredLength} characters long");

            if (policy.RequireLowercase)
            {
                requirements.Add("Contains at least one lowercase letter (a-z)");
            }

            if (policy.RequireUppercase)
            {
                requirements.Add("Contains at least one uppercase letter (A-Z)");
            }

            if (policy.RequireDigit)
            {
                requirements.Add("Contains at least one number (0-9)");
            }

            if (policy.RequireNonAlphanumeric)
            {
                requirements.Add("Contains at least one special character (!@#$%^&* etc.)");
            }

            if (policy.RequiredUniqueChars > 1)
            {
                requirements.Add($"Contains at least {policy.RequiredUniqueChars} unique characters");
            }

            return requirements;
        }
    }
}