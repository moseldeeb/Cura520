using Cura520.Models;
using Cura520.ViewModel;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging.Signing;
using System.Threading.Tasks;

namespace Cura520.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user  = await _userManager.GetUserAsync(User); 
            if (user is null )
            {
                return NotFound();
            }
            //var userVM = new ApplicationUserVM()
            //{
            //    Email = user.Email,
            //    PhoneNumber = user.PhoneNumber,
            //    Address = user.Address,
            //    FullName = $"{user.FirstName} {user.LastName} " 
            //};
            //TypeAdapterConfig<ApplicationUser, ApplicationUserVM>
            //    .NewConfig()
            //    .Map(dest=> dest.FullName , src=> $"{src.FirstName} {src.LastName}");

            var userVM = user.Adapt<ApplicationUserVM>(); 
            return View(userVM);
        }
        public async Task<IActionResult> UpdateProfile(ApplicationUserVM applicationUserVM)
        {
            //var user = applicationUserVM.Adapt<ApplicationUser>();

            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }
            //user.Email = applicationUserVM.Email;
            user.PhoneNumber = applicationUserVM.PhoneNumber;
            user.Address = applicationUserVM.Address;
            var names = applicationUserVM.FullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = names?.Length > 0 ? names[0] : "";
            user.LastName = names?.Length > 1 ? names[1] : "";
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["Error"] = String.Join(", ", result.Errors.Select(e => e.Description));

                return View("Index", applicationUserVM);

            }
        }
        public async Task<IActionResult> UpdatePassword(ApplicationUserVM applicationUserVM)
        {
            if (String.IsNullOrEmpty(applicationUserVM.CurrentPassword) || String.IsNullOrEmpty(applicationUserVM.NewPassword))
            {
                TempData["Error"] = "Current password and New password are required.";
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);   
            var result = await _userManager.ChangePasswordAsync(user, applicationUserVM.CurrentPassword , applicationUserVM.NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"]= "Password updated successfully.";
            }
            else
            {
                TempData["Error"] = String.Join(", ", result.Errors.Select(e => e.Description));
            }
            return RedirectToAction(nameof(Index));

        }

    }
}
