using BeachSAWeb.Models;
using BeachSAWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace BeachSAWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly ApiService _apiService = null;

        private readonly HttpClient _httpClient = null;

        public UserController()
        {
            this._apiService = new ApiService();
            this._httpClient = this._apiService.ApiBeachStart();
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] User temp, bool remember, string returnUrl)
        {
            AuthorizationResponse authorization = null;
            ViewData["ReturnUrl"] = returnUrl;

            if (temp == null)
            {
                TempData["ErrorMessage"] = "No hay datos para la autenticación";
                return View(temp);
            }

            try
            {
                HttpResponseMessage http = await
                    _httpClient.PostAsJsonAsync<User>("/api/users/AutenticacionUsuario", temp);

                if (http.IsSuccessStatusCode)
                {
                    var result = http.Content.ReadAsStringAsync().Result;
                    authorization = JsonConvert.DeserializeObject<AuthorizationResponse>(result);

                    if (authorization != null && authorization.Result == true)
                    {
                        User profile = await this.SearchProfile(temp.Email, authorization.Token);

                        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                        identity.AddClaim(new Claim(ClaimTypes.Name, temp.Email));
                        identity.AddClaim(new Claim("FullName", profile?.FullName ?? temp.Email));
                        identity.AddClaim(new Claim(ClaimTypes.Role, profile?.Role ?? "Customer"));
                        identity.AddClaim(new Claim("IdType", profile?.IdType ?? ""));
                        identity.AddClaim(new Claim("Phone", profile != null && profile.PhoneNumber > 0 ? profile.PhoneNumber.ToString() : ""));
                        var mainSession = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme, mainSession,
                            new AuthenticationProperties { IsPersistent = remember });

                        HttpContext.Session.SetString("token", authorization.Token);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }

                TempData["ErrorMessage"] = "Usuario o contraseña incorrectos.";
                return View(temp);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
                return View(temp);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind] User temp)
        {
            if (temp == null)
            {
                TempData["ErrorMessage"] = "No se permiten datos en blanco";
                return View(temp);
            }

            temp.Id = 0;
            temp.Role = "Customer";

            if (string.IsNullOrEmpty(temp.IdType))
            {
                temp.IdType = "FISICA";
            }

            if (string.IsNullOrEmpty(temp.Address))
            {
                temp.Address = "-";
            }

            try
            {
                HttpResponseMessage http = await _httpClient.PostAsJsonAsync<User>("/api/users/Create", temp);

                if (http.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cuenta creada. Ya puede iniciar sesión.";
                    return RedirectToAction("Login");
                }

                TempData["ErrorMessage"] = $"No se pudo crear la cuenta: {http.Content.ReadAsStringAsync().Result}";
                return View(temp);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "No hay conexión con la API. Inténtalo más tarde.";
                return View(temp);
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchCedula(string cedula)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://apis.gometa.org");

                    HttpResponseMessage http = await client.GetAsync($"/cedulas/{cedula}");

                    if (http.IsSuccessStatusCode)
                    {
                        var result = http.Content.ReadAsStringAsync().Result;
                        JObject data = JObject.Parse(result);

                        JToken person = data["results"]?.FirstOrDefault();

                        if (person != null)
                        {
                            string fullName = (string)data["nombre"] ?? (string)person["fullname"];
                            string idType = (string)person["guess_type"] ?? "FISICA";

                            return Json(new { found = true, fullName = fullName, idType = idType });
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return Json(new { found = false });
        }

        private async Task<User> SearchProfile(string email, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage http = await _httpClient.GetAsync($"/api/users/SearchEmail?email={Uri.EscapeDataString(email)}");

                if (http.IsSuccessStatusCode)
                {
                    var result = http.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<User>(result);
                }
            }
            catch (HttpRequestException)
            {
            }

            return null;
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "User");
        }
    }
}
