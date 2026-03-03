using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}")]

    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users  = _userManager.Users.ToList();
            return View(users);
        }
        public async Task<IActionResult> LockUnLock(string id )
        {
            var user = await _userManager.FindByIdAsync(id); 
            if (user == null)
            {
                return NotFound();
            }
            if (await _userManager.IsInRoleAsync(user  , SD.SUPER_ADMIN_ROLE))
            {
                TempData["error"] = "Cannot lock/unlock Super Admin user.";
                return RedirectToAction(nameof(Index));
            }
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                user.LockoutEnd = null; 
                TempData["success"] = "User unlocked successfully.";
            }
            else
            {
                user.LockoutEnd = DateTime.UtcNow.AddYears(1);
                TempData["success"] = "User locked successfully.";
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));  
        }
    }
}
