using AirlineManager.DataAccess.Data;
using AirlineManager.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AirlineManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperAdmin")]
    public class ConfigurationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly string EncryptionKey = "AirlineManager2025SecretKey123!"; // W produkcji u¿ywaj appsettings/secrets

        public ConfigurationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string category = null)
        {
            var query = _context.AppConfigurations.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(c => c.Category == category);
            }

            var configurations = await query.OrderBy(c => c.Category).ThenBy(c => c.Key).ToListAsync();
            var categories = await _context.AppConfigurations.Select(c => c.Category).Distinct().OrderBy(c => c).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            return View(configurations);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var config = await _context.AppConfigurations.FindAsync(id);
            if (config == null) return NotFound();

            // Decrypt value if encrypted
            if (config.IsEncrypted && !string.IsNullOrEmpty(config.Value))
            {
                try
                {
                    config.Value = DecryptString(config.Value);
                }
                catch
                {
                    // If decryption fails, keep encrypted value
                }
            }

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppConfiguration model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var config = await _context.AppConfigurations.FindAsync(id);
            if (config == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);

            config.Value = model.Value;
            config.Description = model.Description;

            // Encrypt value if needed
            if (config.IsEncrypted && !string.IsNullOrEmpty(config.Value))
            {
                config.Value = EncryptString(config.Value);
            }

            config.LastModified = DateTime.UtcNow;
            config.LastModifiedBy = currentUser?.Email ?? "Unknown";

            await _context.SaveChangesAsync();

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = $"Configuration '{config.Key}' updated successfully.";

            return RedirectToAction(nameof(Index), new { category = config.Category });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestSmtp()
        {
            try
            {
                var smtpConfig = await _context.AppConfigurations
           .Where(c => c.Category == "SMTP")
                .ToDictionaryAsync(c => c.Key, c => c.Value);

                var host = smtpConfig.GetValueOrDefault("SMTP_Host");
                var port = int.Parse(smtpConfig.GetValueOrDefault("SMTP_Port", "587"));
                var username = smtpConfig.GetValueOrDefault("SMTP_Username");
                var password = smtpConfig.GetValueOrDefault("SMTP_Password");
                var fromEmail = smtpConfig.GetValueOrDefault("SMTP_FromEmail");
                var fromName = smtpConfig.GetValueOrDefault("SMTP_FromName");
                var enableSsl = bool.Parse(smtpConfig.GetValueOrDefault("SMTP_EnableSSL", "true"));

                // Decrypt password if encrypted
                if (!string.IsNullOrEmpty(password))
                {
                    try
                    {
                        password = DecryptString(password);
                    }
                    catch { }
                }

                // Test SMTP connection
                using var client = new System.Net.Mail.SmtpClient(host, port);
                client.EnableSsl = enableSsl;
                client.Credentials = new System.Net.NetworkCredential(username, password);

                // Send test email to current user
                var currentUser = await _userManager.GetUserAsync(User);
                var message = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(fromEmail, fromName),
                    Subject = "SMTP Test - AirlineManager",
                    Body = $"This is a test email sent at {DateTime.Now:yyyy-MM-dd HH:mm:ss}. Your SMTP configuration is working correctly.",
                    IsBodyHtml = false
                };
                message.To.Add(currentUser.Email);

                await client.SendMailAsync(message);

                TempData["ToastType"] = "success";
                TempData["ToastMessage"] = $"Test email sent successfully to {currentUser.Email}";
            }
            catch (Exception ex)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = $"SMTP test failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { category = "SMTP" });
        }

        private string EncryptString(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16]; // Use a proper IV in production

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private string DecryptString(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16]; // Use a proper IV in production

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}