using AirlineManager.Models.Domain;
using AirlineManager.Models.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AirlineManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign the User role by default
                await _userManager.AddToRoleAsync(user, "User");

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // after successful sign-in, check flag
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null && user.MustChangePassword)
                    {
                        return RedirectToAction(nameof(ChangePassword));
                    }

                    return RedirectToAction("Index", "Home");
                }
                else if (result.RequiresTwoFactor)
                {
                    var userFor2fa = await _userManager.FindByEmailAsync(model.Email);
                    var userIdFor2fa = userFor2fa?.Id;
                    TempData["TwoFactorUserId"] = userIdFor2fa;
                    _logger.LogInformation("Login requires two-factor for {Email}, userId={UserId}", model.Email, userIdFor2fa);
                    return RedirectToAction("LoginWith2fa", new { rememberMe = model.RememberMe, returnUrl = returnUrl, userId = userIdFor2fa });
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "User account locked out.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginWith2fa(bool rememberMe = false, string returnUrl = null, string userId = null)
        {
            if (string.IsNullOrEmpty(userId) && TempData.ContainsKey("TwoFactorUserId"))
                userId = TempData["TwoFactorUserId"]?.ToString();

            var model = new TwoFactorLoginViewModel { RememberMe = rememberMe, ReturnUrl = returnUrl, UserId = userId };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(TwoFactorLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var twoFactorUser = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            ApplicationUser? user = twoFactorUser;

            if (user == null && !string.IsNullOrEmpty(model.UserId))
                user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                TempData["ServerLog"] = "No two-factor context found - please login again.";
                return RedirectToAction("Login");
            }

            var code = model.TwoFactorCode?.Replace(" ", string.Empty).Replace("-", string.Empty) ?? string.Empty;

            if (twoFactorUser != null)
            {
                var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, model.RememberMe, model.RememberMachine);
                if (result.Succeeded) return Redirect(model.ReturnUrl ?? Url.Action("Index", "Home"));
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account locked out.");
                    return View(model);
                }
                ModelState.AddModelError(string.Empty, "Invalid authentication code.");
                return View(model);
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
            if (isValid)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                await _signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = model.RememberMe });
                if (model.RememberMachine) await _signInManager.RememberTwoFactorClientAsync(user);
                return Redirect(model.ReturnUrl ?? Url.Action("Index", "Home"));
            }

            await _userManager.AccessFailedAsync(user);
            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Account locked out.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid authentication code.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginWithRecovery(string returnUrl = null, string userId = null)
        {
            if (string.IsNullOrEmpty(userId) && TempData.ContainsKey("TwoFactorUserId"))
                userId = TempData["TwoFactorUserId"]?.ToString();

            var model = new TwoFactorRecoveryViewModel { ReturnUrl = returnUrl, UserId = userId };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecovery(TwoFactorRecoveryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var twoFactorUser = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            ApplicationUser? user = twoFactorUser;

            if (user == null && !string.IsNullOrEmpty(model.UserId))
                user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null) return RedirectToAction("Login");

            var code = model.RecoveryCode?.Replace(" ", string.Empty) ?? string.Empty;

            if (twoFactorUser != null)
            {
                var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(code);
                TempData["SignInResult"] = result.ToString();
                if (result.Succeeded) return Redirect(model.ReturnUrl ?? Url.Action("Index", "Home"));
                if (result.IsLockedOut) { ModelState.AddModelError(string.Empty, "Account locked out."); return View(model); }
                ModelState.AddModelError(string.Empty, "Invalid recovery code."); return View(model);
            }

            var identityResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, code);
            TempData["SignInResult"] = identityResult.Succeeded ? "Redeem:Succeeded" : "Redeem:Failed";
            if (identityResult.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                await _signInManager.SignInAsync(user, new AuthenticationProperties { IsPersistent = false });
                return Redirect(model.ReturnUrl ?? Url.Action("Index", "Home"));
            }
            if (await _userManager.IsLockedOutAsync(user)) { ModelState.AddModelError(string.Empty, "Account locked out."); return View(model); }
            ModelState.AddModelError(string.Empty, "Invalid recovery code."); return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // prepare two-factor info
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            string? authenticatorUri = null;
            if (!string.IsNullOrEmpty(unformattedKey))
            {
                authenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey);
            }

            var is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            int? recoveryCodesLeft = null;
            if (is2faEnabled)
            {
                recoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);
            }

            var model = new ProfileCompositeViewModel
            {
                Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                Email = new ProfileEmailViewModel { Email = user.Email },
                Password = new ProfilePasswordViewModel(),
                Delete = new ProfileDeleteViewModel(),
                TwoFactor = new
                {
                    Is2faEnabled = is2faEnabled,
                    SharedKey = unformattedKey,
                    AuthenticatorUri = authenticatorUri,
                    QrCodeImage = (string?)null,
                    RecoveryCodes = (IEnumerable<string>?)null,
                    RecoveryCodesLeft = recoveryCodesLeft
                }
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [Route("Account/Generate2faAjax")]
        public async Task<IActionResult> Generate2faAjax()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Not authenticated" });

            // Always reset authenticator key to generate a new one each time
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

            var authenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey);

            return Json(new { success = true, authenticatorUri, sharedKey = unformattedKey });
        }

        [HttpPost]
        [Authorize]
        [IgnoreAntiforgeryToken]
        [Route("Account/EnableSecondFactorAjax")]
        public async Task<IActionResult> EnableSecondFactorAjax([FromBody] JsonElement payload)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Not authenticated" });

            if (!payload.TryGetProperty("VerificationCode", out var codeProp))
            {
                return Json(new { success = false, message = "VerificationCode missing" });
            }

            var code = codeProp.GetString() ?? string.Empty;
            code = code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
            if (!isValid)
            {
                return Json(new { success = false, message = "Invalid verification code" });
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return Json(new { success = true, recovery = codes });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorSelf()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _userManager.UpdateSecurityStampAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Two-factor authentication disabled.";
            TempData["ActiveTab"] = "twofactor";

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        [Route("Account/ResetRecoveryCodesAjax")]
        public async Task<IActionResult> ResetRecoveryCodesAjax()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Not authenticated" });

            var is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!is2faEnabled)
            {
                return Json(new { success = false, message = "Two-factor authentication is not enabled" });
            }

            var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            _logger.LogInformation("Recovery codes reset for user {Email}", user.Email);

            return Json(new { success = true, recovery = codes });
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            var issuer = UrlEncoder.Default.Encode("AirlineManager");
            var user = UrlEncoder.Default.Encode(email);
            return $"otpauth://totp/{issuer}:{user}?secret={unformattedKey}&issuer={issuer}&digits=6";
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileInfo([Bind(Prefix = "Info")] ProfileInfoViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["ActiveTab"] = "info";
                var composite = new ProfileCompositeViewModel
                {
                    Info = model,
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = new ProfilePasswordViewModel(),
                    Delete = new ProfileDeleteViewModel()
                };
                return View("Profile", composite);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Profile information updated.";
            TempData["ActiveTab"] = "info";

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfileEmail([Bind(Prefix = "Email")] ProfileEmailViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["ActiveTab"] = "email";
                var compositeErr = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = model,
                    Password = new ProfilePasswordViewModel(),
                    Delete = new ProfileDeleteViewModel()
                };
                return View("Profile", compositeErr);
            }

            var pwValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!pwValid)
            {
                ModelState.AddModelError("Email.CurrentPassword", "Invalid current password.");
                TempData["ActiveTab"] = "email";
                var compositeErr = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = model,
                    Password = new ProfilePasswordViewModel(),
                    Delete = new ProfileDeleteViewModel()
                };
                return View("Profile", compositeErr);
            }

            var setEmail = await _userManager.SetEmailAsync(user, model.Email);
            if (!setEmail.Succeeded)
            {
                foreach (var err in setEmail.Errors) ModelState.AddModelError(string.Empty, err.Description);
                TempData["ActiveTab"] = "email";
                var compositeErr = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = model,
                    Password = new ProfilePasswordViewModel(),
                    Delete = new ProfileDeleteViewModel()
                };
                return View("Profile", compositeErr);
            }

            user.UserName = model.Email;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Email updated.";
            TempData["ActiveTab"] = "email";

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilePassword([Bind(Prefix = "Password")] ProfilePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["ActiveTab"] = "password";
                var compositeErr = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = model,
                    Delete = new ProfileDeleteViewModel()
                };
                return View("Profile", compositeErr);
            }

            var change = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!change.Succeeded)
            {
                foreach (var err in change.Errors) ModelState.AddModelError(string.Empty, err.Description);
                TempData["ActiveTab"] = "password";
                var compositeErr = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = model,
                    Delete = new ProfileDeleteViewModel()
                };
                return View("Profile", compositeErr);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Password changed.";
            TempData["ActiveTab"] = "password";

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var export = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                Roles = roles,
                Claims = claims.Select(c => new { c.Type, c.Value })
            };
            var json = JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"user-{user.Id}.json");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount([Bind(Prefix = "Delete")] ProfileDeleteViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["ActiveTab"] = "export";
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = new ProfilePasswordViewModel(),
                    Delete = model
                };
                return View("Profile", composite);
            }

            var pwOk = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!pwOk)
            {
                ModelState.AddModelError("Delete.CurrentPassword", "Invalid password.");
                TempData["ActiveTab"] = "export";
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = new ProfilePasswordViewModel(),
                    Delete = model
                };
                return View("Profile", composite);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ToastType"] = "error";
                TempData["ToastMessage"] = "Failed to delete account.";
                TempData["ActiveTab"] = "export";
                return RedirectToAction("Profile");
            }

            await _signInManager.SignOutAsync();
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Your account has been deleted.";

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectionToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!resetResult.Succeeded)
            {
                foreach (var err in resetResult.Errors) ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            user.MustChangePassword = false;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Your password has been changed.";

            return RedirectToAction("Index", "Home");
        }
    }
}