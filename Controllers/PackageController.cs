using BeachSAWeb.Models;
using BeachSAWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace BeachSAWeb.Controllers
{
    /// <summary>
    /// Controla la consulta y el mantenimiento de paquetes.
    /// </summary>
    [Authorize]
    public class PackageController : Controller
    {
        private readonly ApiService _apiService = null;
        private readonly HttpClient _httpClient = null;

        /// <summary>
        /// Inicializa el cliente HTTP que consume la API.
        /// </summary>
        public PackageController()
        {
            this._apiService = new ApiService();
            this._httpClient = this._apiService.ApiBeachStart();
        }

        /// <summary>
        /// Muestra los paquetes; al administrador le muestra la tabla de mantenimiento.
        /// </summary>
        /// <param name="pName">Texto para buscar paquetes por su tipo.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(string pName)
        {
            List<Package> packages = new List<Package>();

            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            try
            {
                HttpResponseMessage response = null;

                if (string.IsNullOrEmpty(pName))
                {
                    response = await _httpClient.GetAsync("/api/packages/List");
                }
                else
                {
                    response = await _httpClient.GetAsync($"/api/packages/SearchDescrip?packageType={Uri.EscapeDataString(pName)}");
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    packages = JsonConvert.DeserializeObject<List<Package>>(result) ?? new List<Package>();
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudieron cargar los paquetes.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
            }

            ViewData["pName"] = pName;

            if (User.IsInRole("Admin"))
            {
                return View("AdminIndex", packages);
            }

            return View(packages);
        }

        /// <summary>
        /// Crea o modifica un paquete desde la ventana del panel.
        /// </summary>
        /// <param name="temp">Datos enviados desde el formulario.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveModal([Bind] Package temp)
        {
            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se permiten datos en blanco";
                return RedirectToAction("Index");
            }

            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            HttpResponseMessage response = null;

            if (temp.Id > 0)
            {
                response = await _httpClient.PutAsJsonAsync<Package>("/api/packages/Update", temp);
            }
            else
            {
                response = await _httpClient.PostAsJsonAsync<Package>("/api/packages/Create", temp);
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                TempData["ErrorMessage"] = $"Error al guardar el paquete: {response.Content.ReadAsStringAsync().Result}";
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]

        /// <summary>
        /// Muestra el formulario para crear un paquete.
        /// </summary>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]

        /// <summary>
        /// Registra un paquete nuevo en la API.
        /// </summary>
        /// <param name="temp">Datos enviados desde el formulario.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Package temp)
        {
            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se permiten datos en blanco";
                return View(temp);
            }

            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            temp.Id = 0;

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync<Package>("/api/packages/Create", temp);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Error al guardar el paquete: {response.Content.ReadAsStringAsync().Result}";
            return View(temp);
        }

        [Authorize(Roles = "Admin")]

        /// <summary>
        /// Muestra el formulario para modificar un paquete.
        /// </summary>
        /// <param name="id">Identificador del paquete.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            Package temp = await this.SearchPackage(id);

            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se encontró el paquete";
                return RedirectToAction("Index");
            }

            return View(temp);
        }

        [Authorize(Roles = "Admin")]

        /// <summary>
        /// Guarda los cambios de un paquete en la API.
        /// </summary>
        /// <param name="temp">Datos enviados desde el formulario.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Package temp)
        {
            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se permiten datos en blanco";
                return View(temp);
            }

            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            HttpResponseMessage response = await _httpClient.PutAsJsonAsync<Package>("/api/packages/Update", temp);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Error al modificar el paquete: {response.Content.ReadAsStringAsync().Result}";
            return View(temp);
        }

        [Authorize(Roles = "Admin")]

        /// <summary>
        /// Muestra la confirmación para eliminar un paquete.
        /// </summary>
        /// <param name="id">Identificador del paquete.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            Package temp = await this.SearchPackage(id);

            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se encontró el paquete";
                return RedirectToAction("Index");
            }

            return View(temp);
        }

        [Authorize(Roles = "Admin")]

        /// <summary>
        /// Elimina un paquete en la API.
        /// </summary>
        /// <param name="id">Identificador del paquete.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deleted(int id)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/packages/Delete?id={id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                TempData["ErrorMessage"] = $"Error al eliminar el paquete: {response.Content.ReadAsStringAsync().Result}";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Busca un paquete por su identificador.
        /// </summary>
        /// <param name="id">Identificador del paquete.</param>
        /// <returns>El paquete encontrado, o null si no existe.</returns>
        private async Task<Package> SearchPackage(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/packages/SearchById?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Package>(result);
            }

            return null;
        }

        /// <summary>
        /// Arma el encabezado de autorización con el token guardado en la sesión.
        /// </summary>
        /// <returns>El encabezado Bearer, o null si no hay token.</returns>
        private AuthenticationHeaderValue AuthorizationValue()
        {
            string varToken = HttpContext.Session.GetString("token");

            AuthenticationHeaderValue authentication = null;

            if (varToken != null && varToken.Length > 0)
            {
                authentication = new AuthenticationHeaderValue("Bearer", varToken);
            }

            return authentication;
        }

        /// <summary>
        /// Agrega el token de la sesión a las peticiones a la API.
        /// </summary>
        /// <returns>true si había token; false si la sesión expiró.</returns>
        private bool SetToken()
        {
            this._httpClient.DefaultRequestHeaders.Authorization = this.AuthorizationValue();

            if (this._httpClient.DefaultRequestHeaders.Authorization == null)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return false;
            }

            return true;
        }
    }
}
