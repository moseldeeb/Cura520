using Cura520.Models;
using Cura520.Repos;
using Cura520.Utilities;
using Cura520.ViewModel;
using Cura520.ViewModel.Admin.Doctor;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cura520.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_SuperAdmin},{SD.Role_Admin}")]
    public class DoctorController(
        UserManager<ApplicationUser> userManager,
        IRepository<Models.Doctor> doctorRepository,
        IRepository<DoctorSchedule> doctorScheduleRepository) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IRepository<Models.Doctor> _doctorRepository = doctorRepository;
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
            var model = new CreateDoctorVM
            {
                DoctorSchedules = [new()]
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDoctorVM doctorVM)
        {
            if (!ModelState.IsValid)
            {
                return View(doctorVM);
            }

            try
            {
                // Validate that doctor schedules exist
                if (doctorVM.DoctorSchedules == null || !doctorVM.DoctorSchedules.Any())
                {
                    ModelState.AddModelError("", "At least one schedule is required.");
                    return View(doctorVM);
                }

                var Doctor = doctorVM.Adapt<Doctor>();
                
                // Null check for mapped doctor
                if (Doctor == null)
                {
                    ModelState.AddModelError("", "Error mapping doctor data. Please try again.");
                    return View(doctorVM);
                }

                // Handle image file
                if (doctorVM.ImageFile != null && doctorVM.ImageFile.Length > 0)
                {
                    try
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(doctorVM.ImageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Doctors", fileName);

                        var directory = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await doctorVM.ImageFile.CopyToAsync(stream);
                        }
                        Doctor.Img = fileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error uploading image file. Please try again.");
                        return View(doctorVM);
                    }
                }

                var user = doctorVM.Adapt<ApplicationUser>();
                
                if (user == null)
                {
                    ModelState.AddModelError("", "Error creating user account. Please try again.");
                    return View(doctorVM);
                }

                user.UserName = doctorVM.Email;
                user.Type = UserType.Doctor;
                user.EmailConfirmed = true;
                user.PhoneNumberConfirmed = true;

                var result = await _userManager.CreateAsync(user, doctorVM.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Doctor");
                    Doctor.ApplicationUserId = user.Id;

                    // Map schedules with null safety
                    Doctor.DoctorSchedules = [.. doctorVM.DoctorSchedules
                        .Where(s => s != null)
                        .Select(s => new DoctorSchedule
                        {
                            Day = s.Day,
                            StartTime = s.StartTime,
                            EndTime = s.EndTime,
                            Doctor = Doctor
                        })];

                    try
                    {
                        await _doctorRepository.AddAsync(Doctor);
                        await _doctorRepository.CommitAsync();

                        TempData["success"] = "Doctor created successfully.";
                        return RedirectToAction(nameof(Home));
                    }
                    catch (Exception ex)
                    {
                        // Rollback: delete the created user
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError("", "An error occurred while saving the doctor profile. Please try again.");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(doctorVM);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(doctorVM);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var doctorInDB = await _doctorRepository.GetOneAsync(
                d => d.Id == id,
                include: q => q.Include(d => d.DoctorSchedules)
            );

            if (doctorInDB is null) return NotFound();

            var updateDoctor = doctorInDB.Adapt<UpdateDoctorVM>();

            updateDoctor.DoctorSchedules = doctorInDB.DoctorSchedules?.Select(s => new ScheduleVM
            {
                Id = s.Id,
                Day = s.Day,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                DoctorId = s.DoctorId
            }).ToList() ?? [];

            if (!string.IsNullOrEmpty(doctorInDB.ApplicationUserId))
            {
                var user = await _userManager.FindByIdAsync(doctorInDB.ApplicationUserId);
                if (user != null)
                {
                    updateDoctor.Email = user.Email;
                    updateDoctor.FirstName = user.FirstName;
                    updateDoctor.LastName = user.LastName;
                    updateDoctor.PhoneNumber = user.PhoneNumber;
                }
            }

            return View(updateDoctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateDoctorVM doctorVM)
        {
            ModelState.Remove(nameof(UpdateDoctorVM.ImageFile));
            ModelState.Remove(nameof(UpdateDoctorVM.Password));
            ModelState.Remove(nameof(UpdateDoctorVM.ConfirmPassword));

            if (!string.IsNullOrWhiteSpace(doctorVM.Password) &&
                 doctorVM.Password != doctorVM.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(doctorVM.ConfirmPassword), "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                return View(doctorVM);
            }

            var doctorInDB = await _doctorRepository.GetOneAsync(c => c.Id == doctorVM.Id, tracked: false);
            if (doctorInDB is null) return NotFound();

            var userUpdateSuccess = await UpdateDoctorCredentialsAsync(doctorVM);
            if (!userUpdateSuccess) return View(doctorVM); 

            doctorVM.Img = await ProcessDoctorImageAsync(doctorVM, doctorInDB.Img);

            var doctor = new Doctor
            {
                Id = doctorVM.Id,
                ApplicationUserId = doctorVM.ApplicationUserId,
                FirstName = doctorVM.FirstName,
                LastName = doctorVM.LastName,
                Specialty = doctorVM.Specialty,
                PhoneNumber = doctorVM.PhoneNumber,
                ConsultationFee = doctorVM.ConsultationFee,
                Img = doctorVM.Img,
                IsDeleted = false
            };
            await SyncDoctorSchedulesAsync(doctorVM);

            _doctorRepository.Update(doctor);
            await _doctorRepository.CommitAsync();

            return RedirectToAction(nameof(Home));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var doctorInDb = await _doctorRepository.GetOneAsync(c => c.Id == id);
                if (doctorInDb is null)
                {
                    TempData["error"] = "Doctor not found.";
                    return RedirectToAction(nameof(Home));
                }

                var doctorUser = await _userManager.FindByIdAsync(doctorInDb.ApplicationUserId);
                if (doctorUser is null)
                {
                    TempData["error"] = "Associated user account not found.";
                    return RedirectToAction(nameof(Home));
                }

                // Delete schedules
                var schedules = await _doctorScheduleRepository.GetAsync(s => s.DoctorId == doctorInDb.Id);
                if (schedules != null && schedules.Any())
                {
                    foreach (var schedule in schedules)
                    {
                        _doctorScheduleRepository.Delete(schedule);
                    }
                    await _doctorScheduleRepository.CommitAsync();
                }

                // Delete image file with error handling
                if (!string.IsNullOrEmpty(doctorInDb.Img))
                {
                    try
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", doctorInDb.Img);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with deletion
                    }
                }

                // Delete doctor record
                _doctorRepository.Delete(doctorInDb);
                await _doctorRepository.CommitAsync();

                // Delete user account
                var userDeleteResult = await _userManager.DeleteAsync(doctorUser);
                if (!userDeleteResult.Succeeded)
                {
                    TempData["warning"] = "Doctor deleted but user account could not be deleted.";
                    return RedirectToAction(nameof(Home));
                }

                TempData["success"] = "Doctor deleted successfully.";
                return RedirectToAction(nameof(Home));
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while deleting the doctor. Please try again.";
                return RedirectToAction(nameof(Home));
            }
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

        private async Task<bool> UpdateDoctorCredentialsAsync(UpdateDoctorVM doctorVM)
        {
            var doctorUser = await _userManager.FindByIdAsync(doctorVM.ApplicationUserId);
            if (doctorUser == null)
            {
                ModelState.AddModelError("", "Associated user not found.");
                return false;
            }

            doctorUser.FirstName = doctorVM.FirstName;
            doctorUser.LastName = doctorVM.LastName;
            doctorUser.PhoneNumber = doctorVM.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(doctorVM.Email) &&
        !doctorVM.Email.Equals(doctorUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByEmailAsync(doctorVM.Email);
                if (existingUser != null && existingUser.Id != doctorUser.Id)
                {
                    ModelState.AddModelError("", "This email is already in use by another account.");
                    return false;
                }

                doctorUser.Email = doctorVM.Email;
                doctorUser.NormalizedEmail = doctorVM.Email.ToUpperInvariant();
                doctorUser.UserName = doctorVM.Email;
                doctorUser.NormalizedUserName = doctorVM.Email.ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(doctorVM.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(doctorUser);
                var passwordResult = await _userManager.ResetPasswordAsync(doctorUser, token, doctorVM.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return false;
                }
            }

            var result = await _userManager.UpdateAsync(doctorUser);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return false;
            }
            return true;
        }

        private async Task<string> ProcessDoctorImageAsync(UpdateDoctorVM doctorVM, string oldImageName)
        {
            if (doctorVM.ImageFile != null && doctorVM.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(doctorVM.ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doctorVM.ImageFile.CopyToAsync(stream);
                }

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", oldImageName);
                if (System.IO.File.Exists(oldPath) && oldImageName != "defaultImg.png")
                {
                    System.IO.File.Delete(oldPath);
                }
                return fileName;
            }

            return oldImageName; 
        }

        private async Task SyncDoctorSchedulesAsync(UpdateDoctorVM doctorVM)
        {
            var currentSchedules = await _doctorScheduleRepository.GetAsync(s => s.DoctorId == doctorVM.Id);
            var incomingSchedules = doctorVM.DoctorSchedules ?? new List<ScheduleVM>();

            var schedulesToDelete = currentSchedules.Where(db => !incomingSchedules.Any(vm => vm.Id == db.Id)).ToList();
            foreach (var schedule in schedulesToDelete)
            {
                _doctorScheduleRepository.Delete(schedule);
            }

            foreach (var item in incomingSchedules)
            {
                if (item.Id == 0) 
                {
                    await _doctorScheduleRepository.AddAsync(new DoctorSchedule
                    {
                        Day = item.Day,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        DoctorId = doctorVM.Id
                    });
                }
                else 
                {
                    var scheduleToUpdate = currentSchedules.FirstOrDefault(s => s.Id == item.Id);
                    if (scheduleToUpdate != null)
                    {
                        scheduleToUpdate.Day = item.Day;
                        scheduleToUpdate.StartTime = item.StartTime;
                        scheduleToUpdate.EndTime = item.EndTime;
                        _doctorScheduleRepository.Update(scheduleToUpdate);
                    }
                }
            }
        }
    }
}