using Cura520.Models;
using Cura520.Repos;
using Cura520.ViewModel.Admin.Receptionist;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cura520.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = $"{SD.Role_SuperAdmin},{SD.Role_Admin},{SD.Role_Manager}")]
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
        public async Task<IActionResult> Create(CreateReceptionistVM  receptionistVM)
        {
            //if (!ModelState.IsValid) return View(model);


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
                receptionistVM.Img = fileName;
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

                return RedirectToAction(nameof(Home));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(receptionistVM);
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
            var receptionistInDB = await _receptionistRepository.GetOneAsync(
                c => c.Id == receptionistVM.Id, tracked: false
                //,include: q => q.Include(d => d.DoctorSchedules)
                );
            //if (!ModelState.IsValid)
            //{
            //    doctorVM.Img = doctorInDB.Img;
            //    return View(doctorVM);
            //}

            //if (doctorInDB is null) return NotFound();




            //var doctorUser = await _userManager.FindByIdAsync(doctorInDB.ApplicationUserId);
            //if ()
            //{
            //    doctorUser.FirstName = doctorVM.FirstName;
            //    doctorUser.LastName = doctorVM.LastName;
            //    doctorUser.PhoneNumber = doctorVM.PhoneNumber;

            //    // Only update password if the user actually typed a new one
            //    if (!string.IsNullOrWhiteSpace(doctorVM.Password))
            //    {
            //        var token = await _userManager.GeneratePasswordResetTokenAsync(doctorUser);
            //        await _userManager.ResetPasswordAsync(doctorUser, token, doctorVM.Password);
            //    }

            //    var userResult = await _userManager.UpdateAsync(doctorUser);
            //    if (!userResult.Succeeded)
            //    {
            //        foreach (var error in userResult.Errors)
            //            ModelState.AddModelError("", error.Description);
            //        return View(doctorVM);
            //    }
            //}



            var receptionist = receptionistVM.Adapt<Receptionist>();

            if (receptionistVM.ImageFile != null && receptionistVM.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(receptionistVM.ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await receptionistVM.ImageFile.CopyToAsync(stream);
                }

                //if (!string.IsNullOrEmpty(receptionist.Img))
                //{
                receptionist.Img = fileName;

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", receptionistInDB.Img);

                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
                //}
            }
            else
            {
                receptionist.Img = receptionistInDB.Img;
            }
            _receptionistRepository.Update(receptionist);
            await _receptionistRepository.CommitAsync();
            return RedirectToAction(nameof(Home));
        }

        [HttpPost]
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
    }
}