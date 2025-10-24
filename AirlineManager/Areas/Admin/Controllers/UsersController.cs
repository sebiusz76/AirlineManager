using AirlineManager.Models.Domain;
using AirlineManager.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AirlineManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string[] _roleOrder = new[] { "User", "Moderator", "Admin", "SuperAdmin" };

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private string GetHighestRole(IEnumerable<string> roles)
        {
            // returns the role with highest index in roleOrder
            var best = roles.OrderBy(r => Array.IndexOf(_roleOrder, r)).FirstOrDefault();
            return best;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var highest = GetHighestRole(roles);
                var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = highest ?? "User",
                    IsLockedOut = isLocked,
                    LockoutEnd = user.LockoutEnd
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new AdminEditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SelectedRole = GetHighestRole(userRoles) ?? "User",
                AllRoles = allRoles,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Update basic info
            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            await _userManager.UpdateAsync(user);

            // Ensure user has only one role: remove existing and add the selected
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            if (!string.IsNullOrEmpty(model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            // Update lockout
            if (model.IsLockedOut)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent deleting currently logged-in admin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(currentUserId) && currentUserId == user.Id)
            {
                return Forbid();
            }

            await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}