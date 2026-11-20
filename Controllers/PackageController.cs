using Microsoft.AspNetCore.Mvc;

namespace BeachSAWeb.Controllers
{
    public class PackageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
