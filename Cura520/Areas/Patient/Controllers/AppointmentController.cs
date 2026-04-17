using Cura520.Models;
using Cura520.Repos;
using Microsoft.AspNetCore.Mvc;

namespace Cura520.Areas.Patient.Controllers
{
        [Area("Patient")]
        // ? AUTHORIZATION DISABLED FOR TESTING - Uncomment when ready
        // [Authorize]
    public class AppointmentController(
                                IRepository<Models.Doctor> doctorRepository,
                                IRepository<DoctorSchedule> doctorScheduleRepository) : Controller
    {
        private readonly IRepository<Models.Doctor> _doctorRepository = doctorRepository;
        private readonly IRepository<DoctorSchedule> _doctorScheduleRepository = doctorScheduleRepository;
        public IActionResult Index()
        {
            return View();
        }
    }
}
