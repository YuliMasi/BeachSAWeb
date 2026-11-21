using BeachSAWeb.Models;
using BeachSAWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace BeachSAWeb.Controllers
{
    [Authorize]
    public class PackageController : Controller
    {
        private readonly ApiService _apiService = null;
        private readonly HttpClient _httpClient = null;

        public PackageController()
        {
            this._apiService = new ApiService();
            this._httpClient = this._apiService.ApiBeachStart();
        }

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
                    response = await _httpClient.GetAsync("/api/Packages/List");
                }
                else
                {
                    response = await _httpClient.GetAsync($"/api/Packages/SearchDescrip?value={Uri.EscapeDataString(pName)}");
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

            return View(packages);
        }

        [Authorize(Roles = "Admin")]

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]

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

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync<Package>("/api/Packages/Create", temp);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Error al guardar el paquete: {response.Content.ReadAsStringAsync().Result}";
            return View(temp);
        }

        [Authorize(Roles = "Admin")]

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

            HttpResponseMessage response = await _httpClient.PutAsJsonAsync<Package>("/api/Packages/Edit", temp);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = $"Error al modificar el paquete: {response.Content.ReadAsStringAsync().Result}";
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

            Package temp = await this.SearchPackage(id);

            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se encontró el paquete";
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

            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/Packages/Delete?id={id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el paquete: {response.Content.ReadAsStringAsync().Result}";
            }

            return RedirectToAction("Index");
        }

        private async Task<Package> SearchPackage(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/api/Packages/Search?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Package>(result);
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
