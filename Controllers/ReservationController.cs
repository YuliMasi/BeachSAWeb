using BeachSAWeb.Models;
using BeachSAWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace BeachSAWeb.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApiService _apiService = null;
        private readonly HttpClient _httpClient = null;

        public ReservationController()
        {
            this._apiService = new ApiService();
            this._httpClient = this._apiService.ApiBeachStart();
        }

        [HttpGet]
        public async Task<IActionResult> Index(int reservationId)
        {
            //if (!this.SetToken())
            //{
            //    return RedirectToAction("Login", "User");
            //}

            var reservations = new List<Reservation>();

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync("/api/Reservations/List");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    reservations = JsonConvert.DeserializeObject<List<Reservation>>(json) ?? new List<Reservation>();
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudieron cargar las reservas.";
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
            }

            ViewData["ReservationId"] = reservationId;

            return View(reservations);
        }

        //[Authorize(Roles = "Client")]

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        //[Authorize(Roles = "Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Reservation temp)
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

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync<Reservation>("/api/Reservation/Create", temp);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Error al guardar la reserva: {response.Content.ReadAsStringAsync().Result}";
            return View(temp);
        }

        [Authorize(Roles = "Admin")]
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
                TempData["ErrorMessage"] = "No se encontró la reserva";
                return RedirectToAction("Index");
            }

            return View(temp);
        }

        [Authorize(Roles = "Admin")]

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deleted(int id)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/Reservation/Delete?id={id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = $"Error al eliminar la reserva: {response.Content.ReadAsStringAsync().Result}";
            }

            return RedirectToAction("Index");
        }

        private async Task<Reservation> SearchReservation(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/Reservations/Search?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Reservation>(result);
            }

            return null;
        }

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
