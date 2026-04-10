using Cura520.DataAccess;
using Cura520.Models;
using Cura520.Repos;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Cura520.Areas.Patient.Controllers
{
    [Area("Patient")]
    // ? AUTHORIZATION DISABLED FOR TESTING - Uncomment when ready
    // [Authorize]
    public class HomeController(ILogger<HomeController> logger,
                                IRepository<Models.Doctor> doctorRepository,
                                IRepository<DoctorSchedule> doctorScheduleRepository) : Controller

    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly IRepository<Models.Doctor> _doctorRepository = doctorRepository;
        private readonly IRepository<DoctorSchedule> _doctorScheduleRepository = doctorScheduleRepository;




        public ActionResult Index()
        {
            return View();
        }
        public ViewResult NotFoundPage()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
