using Cura520.Models;
using Cura520.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cura520.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_SuperAdmin}, {SD.Role_Admin}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Display list of all users
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                var users = _userManager.Users.ToList();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading users: {ex.Message}");
                TempData["error"] = "An error occurred while loading users.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Lock or unlock a user account
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnLock(string id)
        {
            try
            {
                // Validate ID is provided
                if (string.IsNullOrEmpty(id))
                {
                    TempData["error"] = "Invalid user ID.";
                    return RedirectToAction(nameof(Index));
                }

                // Find the user
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found.");
                    TempData["error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Prevent locking Super Admin users
                if (await _userManager.IsInRoleAsync(user, SD.Role_SuperAdmin))
                {
                    _logger.LogWarning($"Attempted to lock Super Admin user: {user.UserName}");
                    TempData["error"] = "Cannot lock/unlock Super Admin users.";
                    return RedirectToAction(nameof(Index));
                }

                // Toggle lock status
                if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
                {
                    // User is locked - unlock them
                    user.LockoutEnd = null;
                    var result = await _userManager.UpdateAsync(user);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User {user.UserName} unlocked by {User.Identity?.Name}");
                        TempData["success"] = $"User {user.UserName} has been unlocked successfully.";
                    }
                    else
                    {
                        _logger.LogError($"Failed to unlock user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        TempData["error"] = "Failed to unlock user. Please try again.";
                    }
                }
                else
                {
                    // User is unlocked - lock them
                    user.LockoutEnd = DateTime.UtcNow.AddYears(1);
                    var result = await _userManager.UpdateAsync(user);
                    
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User {user.UserName} locked by {User.Identity?.Name}");
                        TempData["success"] = $"User {user.UserName} has been locked successfully.";
                    }
                    else
                    {
                        _logger.LogError($"Failed to lock user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        TempData["error"] = "Failed to lock user. Please try again.";
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in LockUnLock: {ex.Message}");
                TempData["error"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
