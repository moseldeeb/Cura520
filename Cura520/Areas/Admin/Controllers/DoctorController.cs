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
    public class DoctorController(
        UserManager<ApplicationUser> userManager,
        IRepository<Doctor> doctorRepository,
        IRepository<DoctorSchedule> doctorScheduleRepository) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IRepository<Doctor> _doctorRepository = doctorRepository;
        private readonly IRepository<DoctorSchedule> _doctorScheduleRepository = doctorScheduleRepository;

        public async Task<IActionResult> Home()
        {
            var doctors = await _doctorRepository.GetAsync(include: q => q.Include(d => d.DoctorSchedules));
            return View(doctors.AsEnumerable());
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _doctorRepository.GetOneAsync(
                d => d.Id == id,
                include: q => q.Include(d => d.DoctorSchedules)
            );

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new DoctorVM
            {
                Doctor = new Doctor(),
                DoctorSchedules = [new()]
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DoctorVM model, IFormFile? img)
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
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", fileName);

                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await img.CopyToAsync(stream);
                }
                model.Doctor.Img = fileName;
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.Doctor.FirstName,
                LastName = model.Doctor.LastName,
                Type = UserType.Doctor,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Doctor");

                // Link the Doctor profile to the newly created User
                model.Doctor.ApplicationUserId = user.Id;

                // 4. IMPORTANT: Attach the schedules from the VM to the Doctor object
                if (model.DoctorSchedules != null && model.DoctorSchedules.Any())
                {
                    model.Doctor.DoctorSchedules = new List<DoctorSchedule>();
                    foreach (var schedule in model.DoctorSchedules)
                    {
                        // Only add if user actually filled in times
                        if (schedule.Day != default)
                        {
                            model.Doctor.DoctorSchedules.Add(schedule);
                        }
                    }
                }

                await _doctorRepository.AddAsync(model.Doctor);
                await _doctorRepository.CommitAsync();

                return RedirectToAction(nameof(Home));
            }

            // 5. If Identity failed (e.g. Password too weak), add errors to ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var doctor = await _doctorRepository.GetOneAsync(d => d.Id == id, include: q => q.Include(d => d.DoctorSchedules));

            if (doctor is null) return NotFound();

            return View(new DoctorVM()
            {
                Doctor = doctor,
                DoctorSchedules = [.. doctor.DoctorSchedules],
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(DoctorVM model, IFormFile? img)
        {
            var doctorInDB = await _doctorRepository.GetOneAsync(c => c.Id == model.Doctor.Id, tracked: false);
            if (doctorInDB is null) return NotFound();

            if (img != null && img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await img.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(doctorInDB.Img))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", doctorInDB.Img);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
                model.Doctor.Img = fileName;
            }
            else
            {
                model.Doctor.Img = doctorInDB.Img;
            }

            model.Doctor.ApplicationUserId = doctorInDB.ApplicationUserId;

            _doctorRepository.Update(model.Doctor);
            await _doctorRepository.CommitAsync();
            return RedirectToAction(nameof(Home));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _doctorRepository.GetOneAsync(c => c.Id == id);
            if (doctor is null) return NotFound();

            if (!string.IsNullOrEmpty(doctor.Img))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", doctor.Img);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _doctorRepository.Delete(doctor);
            await _doctorRepository.CommitAsync();

            return RedirectToAction(nameof(Home));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _doctorScheduleRepository.GetOneAsync(s => s.Id == id);
            if (schedule == null) return Json(new { success = false, message = "Schedule not found" });

            _doctorScheduleRepository.Delete(schedule);
            await _doctorScheduleRepository.CommitAsync();

            return Json(new { success = true });
        }
    }
}