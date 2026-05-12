using Cura520.Models;
using Cura520.Repos;
using Cura520.Utilities;
using Cura520.ViewModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Cura520.Areas.Admin.Controllers
{
    [Area("Admin")]
    
     [Authorize(Roles = $"{SD.Role_SuperAdmin},{SD.Role_Admin}")]
    public class HomeController(UserManager<ApplicationUser> userManager, 
                                IRepository<Models.Doctor> doctorRepository,
                                IRepository<Models.Patient> patientRepository,
                                IRepository<Appointment> appointmentRepository) : Controller
    {

        private readonly IRepository<Models.Patient> _patientRepository = patientRepository;
        private readonly IRepository<Models.Doctor> _doctorRepository = doctorRepository;
        private readonly IRepository<Appointment> _appointmentRepository = appointmentRepository;

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get only non-deleted records
                var doctors = await _doctorRepository.GetAsync(d => !d.IsDeleted);
                var patients = await _patientRepository.GetAsync(p => !p.IsDeleted);
                var appointments = await _appointmentRepository.GetAsync(a => !a.IsDeleted);

                var dashboardVM = new DashboardVM
                {
                    Doctors = doctors,
                    Patients = patients,
                    Appointments = appointments,
                    DoctorCount = doctors.Count(),
                    PatientCount = patients.Count(),
                    PendingAppointments = appointments.Count(a => a.Status == Status.Pending)
                };

                return View(dashboardVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while loading the dashboard.";
                return View(new DashboardVM());
            }
        }
        public ViewResult NotFoundPage()
        {
            return View();
        }
    }
}
