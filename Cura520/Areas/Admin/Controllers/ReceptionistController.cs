using Cura520.Models;
using Cura520.Repos;
using Cura520.Utilities;
using Cura520.ViewModel;
using Cura520.ViewModel.Admin.Doctor;
using Cura520.ViewModel.Admin.Receptionist;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cura520.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_SuperAdmin},{SD.Role_Admin}")]
    public class ReceptionistController(
        UserManager<ApplicationUser> userManager,
        IRepository<Receptionist> receptionistRepository) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IRepository<Receptionist> _receptionistRepository = receptionistRepository;
        public async Task<IActionResult> Home()
        {
            var receptionists = await _receptionistRepository.GetAsync();
            return View(receptionists.AsEnumerable());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var receptionist = await _receptionistRepository.GetOneAsync(
                r => r.Id == id
            );

            if (receptionist == null)
            {
                return NotFound();
            }

            return View(receptionist);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReceptionistVM receptionistVM)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return View(receptionistVM);
            }

            try
            {
                var Receptionist = receptionistVM.Adapt<Receptionist>();

                if (receptionistVM.ImageFile != null && receptionistVM.ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(receptionistVM.ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", fileName);

                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await receptionistVM.ImageFile.CopyToAsync(stream);
                    }
                    Receptionist.Img = fileName;
                }

                var user = receptionistVM.Adapt<ApplicationUser>();

                user.UserName = receptionistVM.Email;
                user.Type = UserType.Receptionist;
                user.EmailConfirmed = true;
                user.PhoneNumberConfirmed = true;

                var result = await _userManager.CreateAsync(user, receptionistVM.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Receptionist");
                    Receptionist.ApplicationUserId = user.Id;

                    await _receptionistRepository.AddAsync(Receptionist);
                    await _receptionistRepository.CommitAsync();

                    TempData["success"] = "Receptionist created successfully.";
                    return RedirectToAction(nameof(Home));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(receptionistVM);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the receptionist. Please try again.");
                return View(receptionistVM);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var receptionistInDB = await _receptionistRepository.GetOneAsync(
                r => r.Id == id
            );

            if (receptionistInDB is null) return NotFound();
            var updateReceptionist = receptionistInDB.Adapt<UpdateReceptionistVM>();


            if (!string.IsNullOrEmpty(receptionistInDB.ApplicationUserId))
            {
                var user = await _userManager.FindByIdAsync(receptionistInDB.ApplicationUserId);
                if (user != null)
                {
                    updateReceptionist.Email = user.Email;
                    updateReceptionist.FirstName = user.FirstName;
                    updateReceptionist.LastName = user.LastName;
                    updateReceptionist.PhoneNumber = user.PhoneNumber;
                }
            }

            return View(updateReceptionist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateReceptionistVM receptionistVM)
        {
            ModelState.Remove(nameof(UpdateDoctorVM.ImageFile));
            ModelState.Remove(nameof(UpdateDoctorVM.Password));
            ModelState.Remove(nameof(UpdateDoctorVM.ConfirmPassword));

            if (!string.IsNullOrWhiteSpace(receptionistVM.Password) &&
                 receptionistVM.Password != receptionistVM.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(receptionistVM.ConfirmPassword), "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                return View(receptionistVM);
            }

            var receptionistInDB = await _receptionistRepository.GetOneAsync(c => c.Id == receptionistVM.Id, tracked: false);
            if (receptionistInDB is null) return NotFound();
            var userUpdateSuccess = await UpdateReceptionistCredentialsAsync(receptionistVM);
            if (!userUpdateSuccess) return View(receptionistVM);

            receptionistVM.Img = await ProcessReceptionistImageAsync(receptionistVM, receptionistInDB.Img);

            var receptionist = new Receptionist
            {
                Id = receptionistVM.Id,
                ApplicationUserId = receptionistVM.ApplicationUserId,
                FirstName = receptionistVM.FirstName,
                LastName = receptionistVM.LastName,
                PhoneNumber = receptionistVM.PhoneNumber,
                Img = receptionistVM.Img,
                IsDeleted = false
            };

            _receptionistRepository.Update(receptionist);
            await _receptionistRepository.CommitAsync();

            return RedirectToAction(nameof(Home));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var receptionistInDB = await _receptionistRepository.GetOneAsync(c => c.Id == id);
            if (receptionistInDB is null) return NotFound();

            if (!string.IsNullOrEmpty(receptionistInDB.Img))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", receptionistInDB.Img);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _receptionistRepository.Delete(receptionistInDB);
            await _receptionistRepository.CommitAsync();

            return RedirectToAction(nameof(Home));
        }
        private async Task<bool> UpdateReceptionistCredentialsAsync(UpdateReceptionistVM receptionistVM)
        {
            var receptionistUser = await _userManager.FindByIdAsync(receptionistVM.ApplicationUserId);
            if (receptionistUser == null)
            {
                ModelState.AddModelError("", "Associated user not found.");
                return false;
            }

            receptionistUser.FirstName = receptionistVM.FirstName;
            receptionistUser.LastName = receptionistVM.LastName;
            receptionistUser.PhoneNumber = receptionistVM.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(receptionistVM.Email) &&
        !receptionistVM.Email.Equals(receptionistUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByEmailAsync(receptionistVM.Email);
                if (existingUser != null && existingUser.Id != receptionistUser.Id)
                {
                    ModelState.AddModelError("", "This email is already in use by another account.");
                    return false;
                }

                receptionistUser.Email = receptionistVM.Email;
                receptionistUser.NormalizedEmail = receptionistVM.Email.ToUpperInvariant();
                receptionistUser.UserName = receptionistVM.Email;
                receptionistUser.NormalizedUserName = receptionistVM.Email.ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(receptionistVM.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(receptionistUser);
                var passwordResult = await _userManager.ResetPasswordAsync(receptionistUser, token, receptionistVM.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return false;
                }
            }

            var result = await _userManager.UpdateAsync(receptionistUser);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return false;
            }
            return true;
        }
        private async Task<string> ProcessReceptionistImageAsync(UpdateReceptionistVM receptionistVM, string oldImageName)
        {
            if (receptionistVM.ImageFile != null && receptionistVM.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(receptionistVM.ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await receptionistVM.ImageFile.CopyToAsync(stream);
                }

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", oldImageName);
                if (System.IO.File.Exists(oldPath) && oldImageName != "defaultImg.png")
                {
                    System.IO.File.Delete(oldPath);
                }
                return fileName;
            }

            return oldImageName;
        }
    }
}