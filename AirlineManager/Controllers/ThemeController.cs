using AirlineManager.Models.Domain;
using AirlineManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AirlineManager.Controllers
{
    [Authorize]
    public class ThemeController : Controller
    {
        private readonly IThemeService _themeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ThemeController> _logger;

        public ThemeController(
            IThemeService themeService,
            UserManager<ApplicationUser> userManager,
            ILogger<ThemeController> logger)
        {
            _themeService = themeService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetTheme(string theme, string returnUrl = null)
        {
            if (!_themeService.IsValidTheme(theme))
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Invalid theme selection.";
                return Redirect(returnUrl ?? "/");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            try
            {
                await _themeService.SetUserThemeAsync(user.Id, theme);

                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Theme changed to {theme}.";

                _logger.LogInformation("User {Email} changed theme to {Theme}", user.Email, theme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting theme for user {Email}", user.Email);
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Failed to update theme preference.";
            }

            return Redirect(returnUrl ?? "/");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurrentTheme()
        {
            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                userId = user?.Id;
            }

            var theme = await _themeService.GetEffectiveThemeAsync(userId);
            return Json(new { theme });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SetThemeAjax([FromBody] SetThemeRequest request)
        {
            if (!_themeService.IsValidTheme(request.Theme))
            {
                return Json(new { success = false, message = "Invalid theme value" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                await _themeService.SetUserThemeAsync(user.Id, request.Theme);
                _logger.LogInformation("User {Email} changed theme to {Theme} via AJAX", user.Email, request.Theme);
                return Json(new { success = true, theme = request.Theme });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting theme for user {Email}", user.Email);
                return Json(new { success = false, message = "Failed to update theme" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Variants()
        {
            return View();
        }
    }

    public class SetThemeRequest
    {
        public string Theme { get; set; } = "auto";
    }
}