using System;
using System.Linq;
using System.Threading.Tasks;
using Cura520.DataAccess;
using Cura520.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Cura520.Models;


namespace Cura520.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientsController> _logger;

        public object ApplicationUserId { get; private set; }

        public PatientsController(ApplicationDbContext context, ILogger<PatientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Patient/Patients
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients
                .Include(p => p.ApplicationUser)
                .Include(p => p.MedicalHistory)
                .Where(p => !p.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            return View(patients);
        }

        // GET: Patient/Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return BadRequest();

            var patient = await _context.Patients
                .Include(p => p.ApplicationUser)
                .Include(p => p.MedicalHistory)
                    .ThenInclude(m => m.Prescriptions)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null) return NotFound();

            return View(patient);
        }

       
        public async Task<IActionResult> Create([Bind("ApplicationUserId,DateOfBirth,Gender,PhoneNumber,BloodType,Allergies")] Models.Patient patient)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ApplicationUsers = new SelectList(_context.Users.AsNoTracking().ToList(), "Id", "UserName", patient.ApplicationUserId);
                return View(patient);
            }

            patient.CreatedAtIfMissing();
            _context.Add(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        


        // Remove the duplicate Create method (the one without [HttpPost] and [ValidateAntiForgeryToken] attributes)
        // The following method should remain as the only Create action for POST:

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApplicationUserId,DateOfBirth,Gender,PhoneNumber,BloodType,Allergies")] Cura520.Models.Patient patient)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ApplicationUsers = new SelectList(_context.Users.AsNoTracking().ToList(), "Id", "UserName", patient.ApplicationUserId);
                return View(patient);
            }

            patient.CreatedAtIfMissing();
            _context.Add(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Patient/Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return BadRequest();

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null || patient.IsDeleted) return NotFound();

            ViewBag.ApplicationUsers = new SelectList(_context.Users.AsNoTracking().ToList(), "Id", "UserName", patient.ApplicationUserId);
            return View(patient);
        }

        // Replace this line:
        // public async Task<IActionResult> Edit(int id, [Bind("Id,ApplicationUserId,DateOfBirth,Gender,PhoneNumber,BloodType,Allergies,IsDeleted")] Patient patient)

        // With this line, using the fully qualified type name:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ApplicationUserId,DateOfBirth,Gender,PhoneNumber,BloodType,Allergies,IsDeleted")] Cura520.Models.Patient patient)
        {

            if (id != patient.Id) return BadRequest();



            if (!ModelState.IsValid)

            {

                ViewBag.ApplicationUsers = new SelectList(_context.Users.AsNoTracking().ToList(), "Id", "UserName", patient.ApplicationUserId);

                return View(patient);

            }



            try

            {

                _context.Update(patient);

                await _context.SaveChangesAsync();

            }

            catch (DbUpdateConcurrencyException)

            {

                if (!PatientExists(patient.Id)) return NotFound();

                throw;

            }



            return RedirectToAction(nameof(Index));

        }

        // GET: Patient/Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();

            var patient = await _context.Patients
                .Include(p => p.ApplicationUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patient/Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            // Soft delete
            patient.IsDeleted = true;
            _context.Update(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id) =>
            _context.Patients.Any(e => e.Id == id && !e.IsDeleted);
    }

    // If there is a namespace called 'Patient' and a class called 'Patient', 
    // you must use the fully qualified name for the class in the extension method:

    internal static class PatientExtensions
    {
        public static void CreatedAtIfMissing(this Cura520.Models.Patient patient)
        {
            try
            {
                var prop = patient.GetType().GetProperty("CreatedAt");
                if (prop != null && prop.PropertyType == typeof(DateTime))
                {
                    var value = (DateTime)prop.GetValue(patient);
                    if (value == default) prop.SetValue(patient, DateTime.UtcNow);
                }
            }
            catch
            {
                // ignore reflection errors; non-critical
            }
        }
    }
}