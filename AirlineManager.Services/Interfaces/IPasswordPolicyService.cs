namespace AirlineManager.Services.Interfaces
{
    public interface IPasswordPolicyService
    {
        /// <summary>
        /// Gets password policy configuration from database
        /// </summary>
        Task<PasswordPolicyConfiguration> GetPasswordPolicyAsync();

        /// <summary>
        /// Gets password requirements as a list of strings for display
        /// </summary>
        Task<List<string>> GetPasswordRequirementsAsync();
    }

    public class PasswordPolicyConfiguration
    {
        public bool RequireDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public int RequiredLength { get; set; }
        public int RequiredUniqueChars { get; set; }
    }
}