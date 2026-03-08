using System.Diagnostics;

namespace Cura520.Areas.Patient.Controllers
{
    [Area("Patient")]
    // ? AUTHORIZATION DISABLED FOR TESTING - Uncomment when ready
    // [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _context;//= new ApplicationDbContext();
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }



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
