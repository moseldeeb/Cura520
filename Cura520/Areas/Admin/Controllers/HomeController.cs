using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cura520.Areas.Admin.Controllers
{
    [Area("Admin")]
    // ? AUTHORIZATION DISABLED FOR TESTING - Uncomment when ready
    // [Authorize(Roles = $"{SD.Role_SuperAdmin},{SD.Role_Admin},{SD.Role_Manager}")]
    public class HomeController : Controller
    {
        // GET: HomeController
        public ActionResult Index()
        {
            return View();
        }
    }
}
