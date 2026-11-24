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
    /// Controla la consulta, descarga y reenvío de facturas.
    /// </summary>
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly ApiService _apiService = null;
        private readonly HttpClient _httpClient = null;

        /// <summary>
        /// Inicializa el cliente HTTP que consume la API.
        /// </summary>
        public InvoiceController()
        {
            this._apiService = new ApiService();
            this._httpClient = this._apiService.ApiBeachStart();
        }

        /// <summary>
        /// Muestra el listado de facturación del panel de administración.
        /// </summary>
        /// <param name="value">Texto o número por el cual filtrar; si viene vacío se muestran todos.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index(int? value)
        {
            List<Invoice> invoices = new List<Invoice>();

            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            try
            {
                HttpResponseMessage response = null;

                if (value == null || value <= 0)
                {
                    response = await _httpClient.GetAsync("/api/invoices/List");
                }
                else
                {
                    response = await _httpClient.GetAsync($"/api/invoices/SearchDescrip?value={value}");
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    invoices = JsonConvert.DeserializeObject<List<Invoice>>(result) ?? new List<Invoice>();
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudieron cargar las facturas.";
                }

                List<User> users = await this.GetList<User>("/api/users/List");
                List<Package> packages = await this.GetList<Package>("/api/packages/List");
                List<Reservation> reservations = await this.GetList<Reservation>("/Reservations/List");

                foreach (Invoice item in invoices)
                {
                    item.User = users.FirstOrDefault(u => u.Id == item.UserId);
                    item.Package = packages.FirstOrDefault(p => p.Id == item.PackageId);
                    item.Reservation = reservations.FirstOrDefault(r => r.Id == item.ReservationId);
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
            }

            ViewData["value"] = value;

            return View(invoices.OrderByDescending(i => i.InvoiceNumber).ToList());
        }

        /// <summary>
        /// Muestra el detalle de la factura de una reservación.
        /// </summary>
        /// <param name="reservationId">Identificador de la reservación de la factura.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int reservationId)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/api/invoices/SearchDetails?reservationId={reservationId}");

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    InvoiceDetail detail = JsonConvert.DeserializeObject<InvoiceDetail>(result);

                    if (detail != null)
                    {
                        return View(detail);
                    }
                }

                TempData["ErrorMessage"] = "No se encontró la factura de esta reservación.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
            }

            return RedirectToAction("Index", "Reservation");
        }

        /// <summary>
        /// Descarga el PDF de la factura de una reservación.
        /// </summary>
        /// <param name="reservationId">Identificador de la reservación de la factura.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Pdf(int reservationId)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/invoices/Pdf?reservationId={reservationId}");

            if (response.IsSuccessStatusCode)
            {
                byte[] pdf = await response.Content.ReadAsByteArrayAsync();
                string fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                    ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                    ?? $"Factura-{reservationId}.pdf";

                return File(pdf, "application/pdf", fileName);
            }

            TempData["ErrorMessage"] = "No se pudo generar el PDF de la factura.";
            return RedirectToAction("Details", new { reservationId });
        }

        /// <summary>
        /// Reenvía por correo el PDF de la factura al cliente.
        /// </summary>
        /// <param name="reservationId">Identificador de la reservación de la factura.</param>
        /// <param name="returnUrl">Dirección a la que se regresa al terminar.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(int reservationId, string returnUrl)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            HttpResponseMessage response = await _httpClient.PostAsync($"/api/invoices/SendEmail?reservationId={reservationId}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo enviar la factura por correo. Inténtelo más tarde.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", new { reservationId });
        }

        /// <summary>
        /// Consulta un listado en la API y lo convierte al tipo indicado.
        /// </summary>
        /// <param name="url">Ruta del método de la API a consultar.</param>
        /// <returns>La lista obtenida, o una lista vacía si la API falla.</returns>
        private async Task<List<T>> GetList<T>(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<T>>(result) ?? new List<T>();
            }

            return new List<T>();
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
