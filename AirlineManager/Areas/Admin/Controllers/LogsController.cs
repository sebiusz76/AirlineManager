using AirlineManager.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirlineManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class LogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string level = null, DateTime? startDate = null, DateTime? endDate = null, string search = null, int page = 1, int pageSize = 100)
        {
            // Set default date range (last 7 days)
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-7).Date;
            if (!endDate.HasValue)
                endDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

            var query = _context.ApplicationLogs.AsQueryable();

            // Filter by level
            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(l => l.Level == level);
            }

            // Filter by date range
            query = query.Where(l => l.Timestamp >= startDate.Value && l.Timestamp <= endDate.Value);

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.Message.Contains(search) || (l.Exception != null && l.Exception.Contains(search)));
            }

            // Order by timestamp descending (newest first)
            query = query.OrderByDescending(l => l.Timestamp);

            // Pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Levels = await _context.ApplicationLogs
         .Select(l => l.Level)
       .Distinct()
              .OrderBy(l => l)
    .ToListAsync();
            ViewBag.SelectedLevel = level;
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(logs);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOldLogs(int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-days);
                var logsToDelete = await _context.ApplicationLogs
           .Where(l => l.Timestamp < cutoffDate)
                 .ToListAsync();

                _context.ApplicationLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();

                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Deleted {logsToDelete.Count} log entries older than {days} days.";
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = $"Failed to delete logs: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            try
            {
                var allLogs = await _context.ApplicationLogs.ToListAsync();
                _context.ApplicationLogs.RemoveRange(allLogs);
                await _context.SaveChangesAsync();

                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Deleted all {allLogs.Count} log entries.";
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = $"Failed to delete logs: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Download(string level = null, DateTime? startDate = null, DateTime? endDate = null, string search = null)
        {
            // Set default date range (last 7 days)
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddDays(-7).Date;
            if (!endDate.HasValue)
                endDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

            var query = _context.ApplicationLogs.AsQueryable();

            // Filter by level
            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(l => l.Level == level);
            }

            // Filter by date range
            query = query.Where(l => l.Timestamp >= startDate.Value && l.Timestamp <= endDate.Value);

            // Filter by search term
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.Message.Contains(search) || (l.Exception != null && l.Exception.Contains(search)));
            }

            var logs = await query.OrderByDescending(l => l.Timestamp).ToListAsync();

            var sb = new System.Text.StringBuilder();
            foreach (var log in logs)
            {
                sb.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{log.Level}] {log.Message}");
                if (!string.IsNullOrEmpty(log.Exception))
                {
                    sb.AppendLine(log.Exception);
                }
                sb.AppendLine();
            }

            var fileName = $"logs-{startDate.Value:yyyy-MM-dd}-to-{endDate.Value:yyyy-MM-dd}.txt";
            var fileBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(fileBytes, "text/plain", fileName);
        }
    }
}