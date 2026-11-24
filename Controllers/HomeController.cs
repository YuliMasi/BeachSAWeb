using BeachSAWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BeachSAWeb.Controllers
{
    /// <summary>
    /// Controla las páginas públicas del sitio.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Muestra la página de inicio.
        /// </summary>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Muestra la página de privacidad.
        /// </summary>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Muestra la página de error con el identificador de la petición.
        /// </summary>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
