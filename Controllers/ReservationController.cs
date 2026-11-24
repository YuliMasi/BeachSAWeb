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
    /// Controla el registro, la consulta, la modificación y la cancelación de reservaciones.
    /// </summary>
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApiService _apiService = null;
        private readonly HttpClient _httpClient = null;

        /// <summary>
        /// Inicializa el cliente HTTP que consume la API.
        /// </summary>
        public ReservationController()
        {
            this._apiService = new ApiService();
            this._httpClient = this._apiService.ApiBeachStart();
        }

        /// <summary>
        /// Muestra las reservaciones: las propias para el cliente y todas para el administrador.
        /// </summary>
        /// <param name="value">Texto o número por el cual filtrar; si viene vacío se muestran todos.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Index(string value)
        {
            List<Reservation> reservations = new List<Reservation>();

            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            try
            {
                HttpResponseMessage response = null;

                if (string.IsNullOrEmpty(value))
                {
                    response = await _httpClient.GetAsync("/Reservations/List");
                }
                else
                {
                    response = await _httpClient.GetAsync($"/Reservations/SearchDescrip?value={Uri.EscapeDataString(value)}");
                }

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    reservations = JsonConvert.DeserializeObject<List<Reservation>>(result) ?? new List<Reservation>();
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudieron cargar las reservaciones.";
                }

                await this.FillReservationData(reservations);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
            }

            ViewData["value"] = value;

            reservations = reservations.OrderByDescending(r => r.Id).ToList();

            if (User.IsInRole("Admin"))
            {
                return View("AdminIndex", reservations);
            }

            return View(reservations);
        }

        /// <summary>
        /// Muestra el formulario de reservación, con los datos de la reserva rápida si vienen.
        /// </summary>
        /// <param name="packageId">Paquete preseleccionado en el formulario.</param>
        /// <param name="packageType">Tipo de paquete preseleccionado en el formulario.</param>
        /// <param name="checkInDate">Fecha de entrada preseleccionada.</param>
        /// <param name="checkOutDate">Fecha de salida preseleccionada.</param>
        /// <param name="guestQuantity">Cantidad de personas preseleccionada.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Create(int packageId, string packageType, DateTime? checkInDate, DateTime? checkOutDate, int guestQuantity)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            List<Package> packages = await this.LoadPackages();

            if (packageId == 0 && !string.IsNullOrEmpty(packageType))
            {
                packageId = packages.FirstOrDefault(p => PackageInfo.Name(p.PackageType) == PackageInfo.Name(packageType))?.Id ?? 0;
            }

            Reservation temp = new Reservation
            {
                PackageId = packageId,
                CheckInDate = checkInDate ?? DateTime.Today.AddDays(1),
                CheckOutDate = checkOutDate ?? (checkInDate ?? DateTime.Today.AddDays(1)).AddDays(3),
                GuestQuantity = guestQuantity > 0 ? guestQuantity : 2,
                PaymentMethod = "Efectivo"
            };

            await this.FillFormData(packages);

            return View(temp);
        }

        /// <summary>
        /// Registra la reservación en la API y abre su factura.
        /// </summary>
        /// <param name="temp">Datos enviados desde el formulario.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Reservation temp)
        {
            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se permiten datos en blanco";
                return RedirectToAction("Create");
            }

            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            temp.Id = 0;
            temp.UserId = 0;
            temp.AccommodationDays = (temp.CheckOutDate.Date - temp.CheckInDate.Date).Days;

            if (temp.AccommodationDays <= 0)
            {
                TempData["ErrorMessage"] = "La fecha de salida debe ser posterior a la fecha de entrada.";
                await this.FillFormData(await this.LoadPackages());
                return View(temp);
            }

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync<Reservation>("/Reservations/Create", temp);

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    InvoiceDetail detail = JsonConvert.DeserializeObject<InvoiceDetail>(result);

                    if (detail != null && detail.EmailSent)
                    {
                        TempData["SuccessMessage"] = $"Reservación confirmada. La factura se envió en PDF a {detail.Customer?.Email}.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Reservación confirmada, pero no se pudo enviar el correo. Puede reenviarlo desde la factura.";
                    }

                    return RedirectToAction("Details", "Invoice", new { reservationId = detail?.Reservation?.Id ?? 0 });
                }

                TempData["ErrorMessage"] = $"Error al guardar la reservación: {response.Content.ReadAsStringAsync().Result}";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
            }

            await this.FillFormData(await this.LoadPackages());
            return View(temp);
        }

        /// <summary>
        /// Muestra el formulario para modificar una reservación.
        /// </summary>
        /// <param name="id">Identificador de la reservación.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            Reservation temp = await this.SearchReservation(id);

            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se encontró la reservación";
                return RedirectToAction("Index");
            }

            temp.PaymentMethod = ReservationInfo.PaymentName(temp.PaymentMethod);

            await this.FillFormData(await this.LoadPackages());

            return View(temp);
        }

        /// <summary>
        /// Guarda los cambios de una reservación; la API recalcula la factura.
        /// </summary>
        /// <param name="temp">Datos enviados desde el formulario.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Reservation temp)
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

            temp.AccommodationDays = (temp.CheckOutDate.Date - temp.CheckInDate.Date).Days;

            if (temp.AccommodationDays <= 0)
            {
                TempData["ErrorMessage"] = "La fecha de salida debe ser posterior a la fecha de entrada.";
                await this.FillFormData(await this.LoadPackages());
                return View(temp);
            }

            try
            {
                HttpResponseMessage response = await _httpClient.PutAsJsonAsync<Reservation>("/Reservations/Update", temp);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Reservación modificada correctamente.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = $"Error al modificar la reservación: {response.Content.ReadAsStringAsync().Result}";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
            }

            await this.FillFormData(await this.LoadPackages());
            return View(temp);
        }

        /// <summary>
        /// Muestra la confirmación para cancelar una reservación.
        /// </summary>
        /// <param name="id">Identificador de la reservación.</param>
        /// <returns>La vista, el archivo o la redirección que corresponde al resultado.</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            Reservation temp = await this.SearchReservation(id);

            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se encontró la reservación";
                return RedirectToAction("Index");
            }

            await this.FillReservationData(new List<Reservation> { temp });

            return View(temp);
        }

        /// <summary>
        /// Cancela la reservación y su factura en la API.
        /// </summary>
        /// <param name="id">Identificador de la reservación.</param>
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

            HttpResponseMessage response = await _httpClient.DeleteAsync($"/Reservations/Delete?id={id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Reservación eliminada correctamente.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Error al eliminar la reservación: {response.Content.ReadAsStringAsync().Result}";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Busca una reservación por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la reservación.</param>
        /// <returns>La reservación encontrada, o null si no existe o no es del usuario.</returns>
        private async Task<Reservation> SearchReservation(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/Reservations/Search?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrWhiteSpace(result))
                {
                    return JsonConvert.DeserializeObject<Reservation>(result);
                }
            }

            return null;
        }

        /// <summary>
        /// Consulta los paquetes disponibles en la API.
        /// </summary>
        /// <returns>La lista de paquetes, o una lista vacía si la API falla.</returns>
        private async Task<List<Package>> LoadPackages()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("/api/packages/List");

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<Package>>(result) ?? new List<Package>();
            }

            return new List<Package>();
        }

        /// <summary>
        /// Carga los paquetes y el tipo de cambio que usa el formulario.
        /// </summary>
        /// <param name="packages">Paquetes disponibles para el formulario.</param>
        private async Task FillFormData(List<Package> packages)
        {
            ViewBag.Packages = packages;
            ViewBag.ExchangeRate = await ReservationInfo.ExchangeRate();
        }

        /// <summary>
        /// Completa cada reservación con su paquete, su factura y, para el administrador, su cliente.
        /// </summary>
        /// <param name="reservations">Reservaciones a completar con su paquete, factura y cliente.</param>
        private async Task FillReservationData(List<Reservation> reservations)
        {
            List<Package> packages = await this.LoadPackages();
            List<Invoice> invoices = new List<Invoice>();
            List<User> users = new List<User>();

            HttpResponseMessage response = await _httpClient.GetAsync("/api/invoices/List");

            if (response.IsSuccessStatusCode)
            {
                invoices = JsonConvert.DeserializeObject<List<Invoice>>(response.Content.ReadAsStringAsync().Result) ?? new List<Invoice>();
            }

            if (User.IsInRole("Admin"))
            {
                response = await _httpClient.GetAsync("/api/users/List");

                if (response.IsSuccessStatusCode)
                {
                    users = JsonConvert.DeserializeObject<List<User>>(response.Content.ReadAsStringAsync().Result) ?? new List<User>();
                }
            }

            foreach (Reservation item in reservations)
            {
                item.Package = packages.FirstOrDefault(p => p.Id == item.PackageId);
                item.Invoice = invoices.FirstOrDefault(i => i.ReservationId == item.Id);
                item.User = users.FirstOrDefault(u => u.Id == item.UserId);
            }
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
