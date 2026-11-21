using Microsoft.AspNetCore.Mvc;

namespace BeachSAWeb.Controllers
{
    public class ReservationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
