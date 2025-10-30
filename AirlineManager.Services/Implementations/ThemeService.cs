using AirlineManager.DataAccess.Data;
using AirlineManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AirlineManager.Services.Implementations
{
    public class ThemeService : IThemeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationService _configService;
        private readonly ILogger<ThemeService> _logger;

        private static readonly string[] ValidThemes = { "auto", "light", "dark", "dark-soft", "dark-slate", "dark-midnight" };

        public ThemeService(
              ApplicationDbContext context,
          IConfigurationService configService,
           ILogger<ThemeService> logger)
        {
            _context = context;
            _configService = configService;
            _logger = logger;
        }

        public async Task<string> GetDefaultThemeAsync()
        {
            try
            {
                var defaultTheme = await _configService.GetValueAsync("Theme_Default");

                if (string.IsNullOrEmpty(defaultTheme) || !IsValidTheme(defaultTheme))
                {
                    _logger.LogWarning("Invalid or missing default theme configuration. Using 'auto' as fallback.");
                    return "auto";
                }

                return defaultTheme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default theme from configuration");
                return "auto";
            }
        }

        public async Task<string?> GetUserThemeAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            try
            {
                var user = await _context.Users
                        .Where(u => u.Id == userId)
                        .Select(u => u.PreferredTheme)
                        .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user theme for user {UserId}", userId);
                return null;
            }
        }

        public async Task SetUserThemeAsync(string userId, string theme)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (!IsValidTheme(theme))
                throw new ArgumentException($"Invalid theme value: {theme}. Must be one of: {string.Join(", ", ValidThemes)}", nameof(theme));

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found when setting theme: {UserId}", userId);
                    throw new InvalidOperationException("User not found");
                }

                user.PreferredTheme = theme;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Theme updated for user {UserId}: {Theme}", userId, theme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting theme for user {UserId}", userId);
                throw;
            }
        }

        public async Task<string> GetEffectiveThemeAsync(string? userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                var userTheme = await GetUserThemeAsync(userId);
                if (!string.IsNullOrEmpty(userTheme) && IsValidTheme(userTheme))
                {
                    return userTheme;
                }
            }

            return await GetDefaultThemeAsync();
        }

        public bool IsValidTheme(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
                return false;

            return ValidThemes.Contains(theme.ToLowerInvariant());
        }
    }
}