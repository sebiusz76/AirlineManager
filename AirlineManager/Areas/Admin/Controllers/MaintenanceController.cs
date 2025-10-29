using AirlineManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AtLeastSuperAdmin")]
    public class MaintenanceController : Controller
    {
        private readonly IConfigurationService _configService;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(
        IConfigurationService configService,
              ILogger<MaintenanceController> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var enabled = await _configService.GetValueAsync<bool>("Maintenance_Mode_Enabled") ?? false;
            var message = await _configService.GetValueAsync("Maintenance_Mode_Message")
              ?? "We are currently performing scheduled maintenance. Please check back soon.";
            var estimatedEnd = await _configService.GetValueAsync("Maintenance_Mode_EstimatedEnd") ?? "";

            ViewBag.MaintenanceEnabled = enabled;
            ViewBag.MaintenanceMessage = message;
            ViewBag.EstimatedEnd = estimatedEnd;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableMaintenance(string message, string estimatedEnd)
        {
            try
            {
                // Update maintenance mode configuration
                await _configService.SetValueAsync("Maintenance_Mode_Enabled", "true");

                if (!string.IsNullOrEmpty(message))
                {
                    await _configService.SetValueAsync("Maintenance_Mode_Message", message);
                }

                if (!string.IsNullOrEmpty(estimatedEnd))
                {
                    await _configService.SetValueAsync("Maintenance_Mode_EstimatedEnd", estimatedEnd);
                }

                _logger.LogWarning("Maintenance mode ENABLED by {User}", User.Identity?.Name);

                TempData["ToastType"] = "warning";
                TempData["ToastMessage"] = "Maintenance mode has been enabled. Only SuperAdmins can access the site.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling maintenance mode");
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "An error occurred while enabling maintenance mode.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableMaintenance()
        {
            try
            {
                // Disable maintenance mode
                await _configService.SetValueAsync("Maintenance_Mode_Enabled", "false");

                _logger.LogInformation("Maintenance mode DISABLED by {User}", User.Identity?.Name);

                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Maintenance mode has been disabled. The site is now accessible to all users.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling maintenance mode");
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "An error occurred while disabling maintenance mode.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMessage(string message, string estimatedEnd)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    await _configService.SetValueAsync("Maintenance_Mode_Message", message);
                }

                if (estimatedEnd != null) // Allow empty string to clear
                {
                    await _configService.SetValueAsync("Maintenance_Mode_EstimatedEnd", estimatedEnd);
                }

                _logger.LogInformation("Maintenance mode message updated by {User}", User.Identity?.Name);

                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = "Maintenance mode message has been updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance mode message");
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "An error occurred while updating the message.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}