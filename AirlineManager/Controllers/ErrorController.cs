using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AirlineManager.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            _logger.LogWarning("HTTP {StatusCode} error occurred. Path: {Path}",
                          statusCode, statusCodeResult?.OriginalPath ?? "Unknown");

            ViewData["StatusCode"] = statusCode;
            ViewData["OriginalPath"] = statusCodeResult?.OriginalPath;
            ViewData["QueryString"] = statusCodeResult?.OriginalQueryString;

            return View("Error", statusCode);
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionDetails != null)
            {
                _logger.LogError(exceptionDetails.Error,
                  "Unhandled exception occurred. Path: {Path}",
          exceptionDetails.Path);
            }

            ViewData["StatusCode"] = 500;
            return View(500);
        }
    }
}