namespace AirlineManager.Services.Interfaces
{
    /// <summary>
    /// Service for managing user theme preferences
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the default theme from configuration
        /// </summary>
        Task<string> GetDefaultThemeAsync();

        /// <summary>
        /// Gets user's preferred theme. Returns null if user is not authenticated.
        /// </summary>
        Task<string?> GetUserThemeAsync(string userId);

        /// <summary>
        /// Sets user's preferred theme
        /// </summary>
        Task SetUserThemeAsync(string userId, string theme);

        /// <summary>
        /// Gets the effective theme for current user (user preference or default)
        /// </summary>
        Task<string> GetEffectiveThemeAsync(string? userId);

        /// <summary>
        /// Validates if theme value is valid (auto, light, or dark)
        /// </summary>
        bool IsValidTheme(string theme);
    }
}