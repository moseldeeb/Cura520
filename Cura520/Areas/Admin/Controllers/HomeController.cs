using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cura520.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    // [Authorize(Roles = $"{SD.Role_SuperAdmin},{SD.Role_Admin},{SD.Role_Manager}")]
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {
            return View();
        }
        public ViewResult NotFoundPage()
        {
            return View();
        }
    }
}
