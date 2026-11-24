using BeachSAWeb.Models;
using BeachSAWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace BeachSAWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApiService _apiService = null;
        private readonly HttpClient _httpClient = null;

        public AdminController()
        {
            this._apiService = new ApiService();
            this._httpClient = this._apiService.ApiBeachStart();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            List<Reservation> reservations = new List<Reservation>();

            try
            {
                reservations = await this.GetList<Reservation>("/Reservations/List");
                List<Invoice> invoices = await this.GetList<Invoice>("/api/invoices/List");
                List<User> users = await this.GetList<User>("/api/users/List");
                List<Package> packages = await this.GetList<Package>("/api/packages/List");

                foreach (Reservation item in reservations)
                {
                    item.User = users.FirstOrDefault(u => u.Id == item.UserId);
                    item.Package = packages.FirstOrDefault(p => p.Id == item.PackageId);
                    item.Invoice = invoices.FirstOrDefault(i => i.ReservationId == item.Id);
                }

                ViewBag.TotalReservations = reservations.Count;
                ViewBag.TotalSalesUsd = invoices.Sum(i => i.TotalUsd);
                ViewBag.TotalClients = users.Count(u => u.Role != "Admin");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
            }

            return View(reservations.OrderByDescending(r => r.Id).Take(5).ToList());
        }

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
