using AirlineManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineManager.Controllers
{
    [AllowAnonymous]
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
            // Check if maintenance mode is enabled
            var maintenanceEnabled = await _configService.GetValueAsync<bool>("Maintenance_Mode_Enabled") ?? false;

            if (!maintenanceEnabled)
            {
                // Maintenance mode is disabled, redirect to home
                return RedirectToAction("Index", "Home");
            }

            // Get maintenance message and estimated end time
            var message = await _configService.GetValueAsync("Maintenance_Mode_Message")
        ?? "We are currently performing scheduled maintenance. Please check back soon.";
            var estimatedEnd = await _configService.GetValueAsync("Maintenance_Mode_EstimatedEnd") ?? "";

            ViewBag.MaintenanceMessage = message;
            ViewBag.EstimatedEnd = estimatedEnd;

            return View();
        }
    }
}