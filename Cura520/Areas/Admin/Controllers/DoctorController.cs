using Cura520.Models;
using Cura520.Repos;
using Cura520.Utilities;
using Cura520.ViewModel.Admin.Doctor;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Numerics;

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
            var model = new CreateDoctorVM
            {
                DoctorSchedules = [new()] 
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDoctorVM doctorVM)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(doctorVM);
            //}

            var Doctor = doctorVM.Adapt<Doctor>();
            
            if (doctorVM.ImageFile != null && doctorVM.ImageFile.Length > 0)
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

            //var user = new ApplicationUser
            //{
            //    UserName = "Dr. " + model.FirstName + " " + model.LastName,
            //    Email = model.Email,
            //    FirstName = model.FirstName,
            //    LastName = model.LastName,
            //    PhoneNumber = model.PhoneNumber,
            //    PhoneNumberConfirmed = true,
            //    Type = UserType.Doctor,
            //    EmailConfirmed = true
            //};
            var user = doctorVM.Adapt<ApplicationUser>();

            user.UserName = doctorVM.Email; 
            user.Type = UserType.Doctor;
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;


            var result = await _userManager.CreateAsync(user, doctorVM.Password);
            

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Doctor");
                Doctor.ApplicationUserId = user.Id;


                Doctor.DoctorSchedules = [.. doctorVM.DoctorSchedules.Select(s => new DoctorSchedule
                {
                    Day = s.Day,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Doctor = Doctor // Sets the foreign key relationship
                })];

                //var doctorSchedules = doctorVM.DoctorSchedules.Adapt<List<DoctorSchedule>>();

                //if (doctorSchedules != null && doctorSchedules.Any())
                //{
                //    foreach (var schedule in doctorSchedules)
                //    {
                //        schedule.Doctor = Doctor;
                //        Doctor.DoctorSchedules.Add(schedule);
                //    }
                //}

                await _doctorRepository.AddAsync(Doctor);
                await _doctorRepository.CommitAsync();

                return RedirectToAction(nameof(Home));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(doctorVM);
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

            updateDoctor.DoctorSchedules = doctorInDB.DoctorSchedules?.Select(s => new DoctorSchedule
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

            var doctorInDB = await _doctorRepository.GetOneAsync(
                c => c.Id == doctorVM.Id, tracked: false
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



            var doctor = doctorVM.Adapt<Doctor>();

            if (doctorVM.ImageFile != null && doctorVM.ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(doctorVM.ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await doctorVM.ImageFile.CopyToAsync(stream);
                }

                //if (!string.IsNullOrEmpty(doctor.Img))
                //{
                    doctor.Img = fileName;

                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", doctorInDB.Img);
                   
                    if (System.IO.File.Exists(oldPath)) 
                        System.IO.File.Delete(oldPath);
                //}
            }
            else
            {
                doctorVM.Img = doctorInDB.Img;
            }
            _doctorRepository.Update(doctor);
            await _doctorRepository.CommitAsync();
            return RedirectToAction(nameof(Home));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var doctorInDb = await _doctorRepository.GetOneAsync(c => c.Id == id);
            if (doctorInDb is null) return NotFound();

            var doctorUser = await _userManager.FindByIdAsync(doctorInDb.ApplicationUserId);

            if (doctorUser is null) return NotFound();

            if (!string.IsNullOrEmpty(doctorInDb.Img))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Doctors", doctorInDb.Img);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _doctorRepository.Delete(doctorInDb);
            await _userManager.DeleteAsync(doctorUser);
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