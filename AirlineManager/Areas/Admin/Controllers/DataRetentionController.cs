using AirlineManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AtLeastSuperAdmin")]
    public class DataRetentionController : Controller
    {
        private readonly IDataRetentionService _retentionService;
        private readonly ILogger<DataRetentionController> _logger;

        public DataRetentionController(
IDataRetentionService retentionService,
    ILogger<DataRetentionController> logger)
        {
            _retentionService = retentionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var config = await _retentionService.GetRetentionConfigurationAsync();
            var stats = await _retentionService.GetRetentionStatisticsAsync();

            ViewBag.Configuration = config;
            ViewBag.Statistics = stats;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunCleanup()
        {
            try
            {
                _logger.LogInformation("Manual data retention cleanup initiated by {User}", User.Identity?.Name);

                var result = await _retentionService.CleanupAllAsync();

                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Cleanup completed! Deleted {result.TotalDeleted} records in {result.Duration.TotalSeconds:F2} seconds.";

                _logger.LogInformation(
              "Manual cleanup completed: {Total} records deleted (Logs: {Logs}, LoginHistory: {LoginHistory}, AuditLogs: {AuditLogs}, Sessions: {Sessions})",
              result.TotalDeleted,
                   result.ApplicationLogsDeleted,
               result.LoginHistoryDeleted,
         result.AuditLogsDeleted,
                     result.InactiveSessionsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual data retention cleanup");
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "An error occurred during cleanup. Please check the logs.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupApplicationLogs()
        {
            try
            {
                var count = await _retentionService.CleanupApplicationLogsAsync();
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Deleted {count} application log records.";
                _logger.LogInformation("Cleaned up {Count} application logs", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up application logs");
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "An error occurred during cleanup.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupLoginHistory()
        {
            try
            {
                var count = await _retentionService.CleanupLoginHistoryAsync();
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Deleted {count} login history records.";
                _logger.LogInformation("Cleaned up {Count} login history records", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up login history");
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "An error occurred during cleanup.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupAuditLogs()
        {
            try
            {
                var count = await _retentionService.CleanupAuditLogsAsync();
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Deleted {count} audit log records.";
                _logger.LogInformation("Cleaned up {Count} audit logs", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up audit logs");
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "An error occurred during cleanup.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupInactiveSessions()
        {
            try
            {
                var count = await _retentionService.CleanupInactiveSessionsAsync();
                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Deleted {count} inactive session records.";
                _logger.LogInformation("Cleaned up {Count} inactive sessions", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up inactive sessions");
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "An error occurred during cleanup.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}