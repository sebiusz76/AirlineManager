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

        private async Task<string> GetCurrentUserHighestRoleAsync()
        {
            var current = await _userManager.GetUserAsync(User);
            if (current == null) return null;
            var roles = await _userManager.GetRolesAsync(current);
            return GetHighestRole(roles);
        }

        private List<string> GetAllowedRolesForCurrentUser(string currentHighestRole)
        {
            if (string.IsNullOrEmpty(currentHighestRole)) return new List<string>();
            if (currentHighestRole == "SuperAdmin") return _roleOrder.ToList();
            var idx = Array.IndexOf(_roleOrder, currentHighestRole);
            if (idx < 0) return new List<string> { "User" };
            // Allow assigning roles up to and including the current user's role
            return _roleOrder.Take(idx + 1).ToList();
        }

        // Index with filtering and pagination
        public async Task<IActionResult> Index(string search = null, string roleFilter = null, int page = 1, int pageSize = 10)
        {
            // provide roles for filter dropdown
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.AvailableRoles = allRoles;

            // normalize paging inputs
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 100);

            List<ApplicationUser> usersPage;
            int totalCount;

            if (!string.IsNullOrEmpty(roleFilter))
            {
                // get users in role (in-memory)
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleFilter);
                var q = usersInRole.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    var s = search.Trim();
                    q = q.Where(u => (u.Email != null && u.Email.Contains(s))
                                     || (u.FirstName != null && u.FirstName.Contains(s))
                                     || (u.LastName != null && u.LastName.Contains(s)));
                }

                totalCount = q.Count();
                usersPage = q.OrderBy(u => u.Email).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                // query directly from store
                var q = _userManager.Users.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    var s = search.Trim();
                    q = q.Where(u => (u.Email != null && u.Email.Contains(s))
                                     || (u.FirstName != null && u.FirstName.Contains(s))
                                     || (u.LastName != null && u.LastName.Contains(s)));
                }

                totalCount = await q.CountAsync();
                usersPage = await q.OrderBy(u => u.Email).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            }

            var model = new List<AdminUserViewModel>();
            foreach (var user in usersPage)
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

            // paging metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.Search = search;
            ViewBag.RoleFilter = roleFilter;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentHighest = await GetCurrentUserHighestRoleAsync();
            var allowedRoles = GetAllowedRolesForCurrentUser(currentHighest);

            // Prevent editing users with higher role than current (unless SuperAdmin or editing self)
            var targetRoles = await _userManager.GetRolesAsync(user);
            var targetHighest = GetHighestRole(targetRoles);
            var currentUser = await _userManager.GetUserAsync(User);
            var currentIndex = Array.IndexOf(_roleOrder, currentHighest);
            var targetIndex = Array.IndexOf(_roleOrder, targetHighest);
            if (currentHighest != "SuperAdmin" && user.Id != currentUser?.Id && targetIndex > currentIndex)
            {
                return Forbid();
            }

            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            // Only show allowed roles in dropdown (SuperAdmin sees all)
            var rolesToShow = (currentHighest == "SuperAdmin") ? allRoles : allRoles.Where(r => allowedRoles.Contains(r)).ToList();

            var model = new AdminEditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SelectedRole = GetHighestRole(targetRoles) ?? "User",
                AllRoles = rolesToShow,
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

            var currentHighest = await GetCurrentUserHighestRoleAsync();
            var allowedRoles = GetAllowedRolesForCurrentUser(currentHighest);
            var currentUser = await _userManager.GetUserAsync(User);
            var currentIndex = Array.IndexOf(_roleOrder, currentHighest);

            // Prevent editing users with higher role than current (unless SuperAdmin or editing self)
            var targetRolesBefore = await _userManager.GetRolesAsync(user);
            var targetHighestBefore = GetHighestRole(targetRolesBefore);
            var targetIndexBefore = Array.IndexOf(_roleOrder, targetHighestBefore);
            if (currentHighest != "SuperAdmin" && user.Id != currentUser?.Id && targetIndexBefore > currentIndex)
            {
                return Forbid();
            }

            // Validate selected role is allowed for current user
            if (currentHighest != "SuperAdmin")
            {
                if (string.IsNullOrEmpty(model.SelectedRole) || !allowedRoles.Contains(model.SelectedRole))
                {
                    ModelState.AddModelError("SelectedRole", "You cannot assign that role.");
                    // repopulate role list for view
                    model.AllRoles = GetAllowedRolesForCurrentUser(currentHighest);
                    return View(model);
                }
            }

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

            // Set TempData for success toast
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "User updated successfully.";

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

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new AdminSetPasswordViewModel
            {
                Id = user.Id,
                Email = user.Email
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(AdminSetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Remove existing password if any (for external logins) then set new password via reset token
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                // generate reset token and reset
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var res = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!res.Succeeded)
                {
                    foreach (var err in res.Errors) ModelState.AddModelError(string.Empty, err.Description);
                    return View(model);
                }
            }
            else
            {
                var res = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (!res.Succeeded)
                {
                    foreach (var err in res.Errors) ModelState.AddModelError(string.Empty, err.Description);
                    return View(model);
                }
            }

            // Force user to change password at next login
            user.MustChangePassword = true;
            await _userManager.UpdateAsync(user);

            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Password updated successfully. User will be required to change it at next login.";
            return RedirectToAction(nameof(Index));
        }
    }
}