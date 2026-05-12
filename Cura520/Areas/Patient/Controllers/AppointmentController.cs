using Cura520.Models;
using Cura520.Models;
using Cura520.Repos;
using Cura520.ViewModel.Patient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Cura520.Areas.Patient.Controllers
{
    [Area("Patient")]
     [Authorize]
    public class AppointmentController(
        IRepository<Models.Doctor> doctorRepository,
        IRepository<DoctorSchedule> doctorScheduleRepository,
        IRepository<Models.Appointment> appointmentRepository,
        IRepository<Models.Patient> patientRepository,
        ILogger<AppointmentController> logger) : Controller
    {
        private readonly IRepository<Models.Doctor> _doctorRepository = doctorRepository;
        private readonly IRepository<DoctorSchedule> _doctorScheduleRepository = doctorScheduleRepository;
        private readonly IRepository<Models.Appointment> _appointmentRepository = appointmentRepository;
        private readonly IRepository<Models.Patient> _patientRepository = patientRepository;
        private readonly ILogger<AppointmentController> _logger = logger;

        
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get current patient - using a helper method
                var currentPatient = await GetCurrentPatientAsync();
                if (currentPatient == null)
                {
                    TempData["Error"] = "Patient profile not found. Please update your profile.";
                    return RedirectToAction("Index", "Home");
                }

                // Get all appointments for the patient
                var appointments = await _appointmentRepository.GetAsync(
                    a => a.PatientId == currentPatient.Id && !a.IsDeleted,
                    include: q => q.Include(a => a.Doctor).Include(a => a.Patient)
                );

                var appointmentList = appointments.OrderByDescending(a => a.AppointmentDate).ToList();
                return View(appointmentList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Appointment Index: {ex.Message}");
                TempData["Error"] = "An error occurred while loading appointments.";
                return RedirectToAction("Index", "Home");
            }
        }

        
        public async Task<IActionResult> Create()
        {
            try
            {
                var currentPatient = await GetCurrentPatientAsync();
                if (currentPatient == null)
                {
                    TempData["Error"] = "Patient profile not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Get available doctors
                var doctors = await _doctorRepository.GetAsync(d => !d.IsDeleted);
                
                var appointmentVM = new CreateAppointmentVM
                {
                    PatientId = currentPatient.Id,
                    PatientName = $"{currentPatient.FirstName} {currentPatient.LastName}",
                    Status = "Pending"
                };

                // Pass doctors to view
                ViewBag.Doctors = doctors.ToList();

                return View(appointmentVM);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Appointment Create GET: {ex.Message}");
                TempData["Error"] = "An error occurred while preparing the appointment form.";
                return RedirectToAction("Index");
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentVM appointmentVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var doctors = await _doctorRepository.GetAsync(d => !d.IsDeleted);
                    ViewBag.Doctors = doctors.ToList();
                    return View(appointmentVM);
                }

                var currentPatient = await GetCurrentPatientAsync();
                if (currentPatient == null)
                {
                    ModelState.AddModelError(string.Empty, "Patient profile not found.");
                    return View(appointmentVM);
                }

                // Verify doctor exists and is not deleted
                var doctor = await _doctorRepository.GetOneAsync(d => d.Id == appointmentVM.DoctorId && !d.IsDeleted);
                if (doctor == null)
                {
                    ModelState.AddModelError(string.Empty, "Selected doctor is not available.");
                    var doctors = await _doctorRepository.GetAsync(d => !d.IsDeleted);
                    ViewBag.Doctors = doctors.ToList();
                    return View(appointmentVM);
                }

                // Create appointment
                var appointment = new Models.Appointment
                {
                    SymptomSummary = appointmentVM.SymptomSummary,
                    AppointmentDate = appointmentVM.AppointmentDate,
                    PatientId = currentPatient.Id,
                    DoctorId = appointmentVM.DoctorId,
                    Status = Status.Pending,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _appointmentRepository.AddAsync(appointment);
                await _appointmentRepository.CommitAsync();

                TempData["Success"] = "Appointment request submitted successfully. Please wait for confirmation.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Appointment Create POST: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the appointment.");
                
                var doctors = await _doctorRepository.GetAsync(d => !d.IsDeleted);
                ViewBag.Doctors = doctors.ToList();
                return View(appointmentVM);
            }
        }

        
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetOneAsync(
                    a => a.Id == id && !a.IsDeleted,
                    include: q => q.Include(a => a.Doctor).Include(a => a.Patient)
                );

                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Verify patient owns this appointment
                var currentPatient = await GetCurrentPatientAsync();
                if (currentPatient?.Id != appointment.PatientId)
                {
                    TempData["Error"] = "You do not have permission to view this appointment.";
                    return RedirectToAction(nameof(Index));
                }

                return View(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Appointment Details: {ex.Message}");
                TempData["Error"] = "An error occurred while retrieving the appointment.";
                return RedirectToAction(nameof(Index));
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetOneAsync(a => a.Id == id && !a.IsDeleted);
                
                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found.";
                    return RedirectToAction(nameof(Index));
                }

                var currentPatient = await GetCurrentPatientAsync();
                if (currentPatient?.Id != appointment.PatientId)
                {
                    TempData["Error"] = "You do not have permission to cancel this appointment.";
                    return RedirectToAction(nameof(Index));
                }

                if (appointment.Status == Status.Completed)
                {
                    TempData["Error"] = "Cannot cancel a completed appointment.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Cancel appointment
                appointment.Status = Status.Cancelled;
                appointment.UpdatedAt = DateTime.Now;

                _appointmentRepository.Update(appointment);
                await _appointmentRepository.CommitAsync();

                TempData["Success"] = "Appointment cancelled successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Appointment Cancel: {ex.Message}");
                TempData["Error"] = "An error occurred while cancelling the appointment.";
                return RedirectToAction(nameof(Index));
            }
        }

        
        private async Task<Models.Patient> GetCurrentPatientAsync()
        {
            try
            {
                // Get current user ID from claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    return null;

                // Find patient with this ApplicationUserId
                var patient = await _patientRepository.GetOneAsync(
                    p => p.ApplicationUserId == userId && !p.IsDeleted
                );

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting current patient: {ex.Message}");
                return null;
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAvailableDoctors()
        {
            try
            {
                var doctors = await _doctorRepository.GetAsync(d => !d.IsDeleted);
                
                var doctorList = doctors.Select(d => new
                {
                    id = d.Id,
                    name = $"Dr. {d.FirstName} {d.LastName}",
                    specialty = d.Specialty,
                    consultationFee = d.ConsultationFee
                }).ToList();

                return Json(doctorList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting available doctors: {ex.Message}");
                return Json(new { error = "Error loading doctors" });
            }
        }
    }
}

