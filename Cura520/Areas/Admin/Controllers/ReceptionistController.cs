using Cura520.Models;
using Cura520.Repos;
using Cura520.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

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
            var model = new ReceptionistVM
            {
                Receptionist = new Receptionist()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReceptionistVM model, IFormFile? img)
        {
            // 1. Check if the model is valid
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            // 2. Handle Image Upload
            if (img != null && img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", fileName);

                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await img.CopyToAsync(stream);
                }
                model.Receptionist.Img = fileName;
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.Receptionist.FirstName,
                LastName = model.Receptionist.LastName,
                Type = UserType.Receptionist,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);


            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Receptionist");
                model.Receptionist.ApplicationUserId = user.Id;

                await _receptionistRepository.AddAsync(model.Receptionist);
                await _receptionistRepository.CommitAsync();

                return RedirectToAction(nameof(Home));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var receptionist = await _receptionistRepository.GetOneAsync(r => r.Id == id);

            if (receptionist is null) return NotFound();

            return View(new ReceptionistVM()
            {
                Receptionist = receptionist
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ReceptionistVM model, IFormFile? img)
        {
            var receptionistInDB = await _receptionistRepository.GetOneAsync(c => c.Id == model.Receptionist.Id, tracked: false);
            if (receptionistInDB is null) return NotFound();

            if (img != null && img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await img.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(receptionistInDB.Img))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", receptionistInDB.Img);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                model.Receptionist.Img = fileName;
            }
            else
            {
                model.Receptionist.Img = receptionistInDB.Img;
            }

            model.Receptionist.ApplicationUserId = receptionistInDB.ApplicationUserId;

            _receptionistRepository.Update(model.Receptionist);
            await _receptionistRepository.CommitAsync();
            return RedirectToAction(nameof(Home));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var receptionist = await _receptionistRepository.GetOneAsync(c => c.Id == id);
            if (receptionist is null) return NotFound();

            if (!string.IsNullOrEmpty(receptionist.Img))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Receptionists", receptionist.Img);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _receptionistRepository.Delete(receptionist);
            await _receptionistRepository.CommitAsync();

            return RedirectToAction(nameof(Home));
        }
    }
}