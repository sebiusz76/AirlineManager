using AirlineManager.Models.Domain;
using AirlineManager.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AirlineManager.Controllers
{
    public partial class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
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
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

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
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new ProfileCompositeViewModel
            {
                Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                Email = new ProfileEmailViewModel { Email = user.Email },
                Password = new ProfilePasswordViewModel(),
                Delete = new ProfileDeleteViewModel()
            };

            return View(model);
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
                // Return composite view with posted info so validation messages display
                var composite = new ProfileCompositeViewModel
                {
                    Info = model,
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = new ProfilePasswordViewModel(),
                    Delete = new ProfileDeleteViewModel()
                };
                TempData["ActiveTab"] = "info";
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
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = model,
                    Password = new ProfilePasswordViewModel(),
                    Delete = new ProfileDeleteViewModel()
                };
                TempData["ActiveTab"] = "email";
                return View("Profile", composite);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!passwordValid)
            {
                ModelState.AddModelError("Email.CurrentPassword", "Invalid current password.");
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = model,
                    Password = new ProfilePasswordViewModel(),
                    Delete = new ProfileDeleteViewModel()
                };
                TempData["ActiveTab"] = "email";
                return View("Profile", composite);
            }

            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
            {
                foreach (var err in setEmailResult.Errors) ModelState.AddModelError(string.Empty, err.Description);
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = model,
                    Password = new ProfilePasswordViewModel(),
                    Delete = new ProfileDeleteViewModel()
                };
                TempData["ActiveTab"] = "email";
                return View("Profile", composite);
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
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = model,
                    Delete = new ProfileDeleteViewModel()
                };
                TempData["ActiveTab"] = "password";
                return View("Profile", composite);
            }

            var changeRes = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changeRes.Succeeded)
            {
                foreach (var err in changeRes.Errors) ModelState.AddModelError(string.Empty, err.Description);
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = model,
                    Delete = new ProfileDeleteViewModel()
                };
                TempData["ActiveTab"] = "password";
                return View("Profile", composite);
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
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = new ProfilePasswordViewModel(),
                    Delete = model
                };
                TempData["ActiveTab"] = "export";
                return View("Profile", composite);
            }

            var pwOk = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!pwOk)
            {
                ModelState.AddModelError("Delete.CurrentPassword", "Invalid password.");
                var composite = new ProfileCompositeViewModel
                {
                    Info = new ProfileInfoViewModel { FirstName = user.FirstName, LastName = user.LastName },
                    Email = new ProfileEmailViewModel { Email = user.Email },
                    Password = new ProfilePasswordViewModel(),
                    Delete = model
                };
                TempData["ActiveTab"] = "export";
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
    }
}