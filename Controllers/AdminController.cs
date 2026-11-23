using BeachSAWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeachSAWeb.Controllers
{
    [AllowAnonymous]
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.TotalReservations = 128;
            ViewBag.TotalSalesUsd = 0m;
            ViewBag.TotalClients = 342;

            return View(new List<Reservation>());
        }
    }
}
