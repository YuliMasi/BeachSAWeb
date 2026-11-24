using BeachSAWeb.Models;
using BeachSAWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
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

            temp.FullName = temp.Email;
            temp.Role = "Customer";
            temp.IdType = "-";
            temp.Address = "-";

            try
            {
                HttpResponseMessage http = await
                    _httpClient.PostAsJsonAsync<User>("/api/users/AuthenticationUser", temp);

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
                HttpResponseMessage http = await _httpClient.PostAsJsonAsync<User>("/api/users/Register", temp);

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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Clients(string name)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            List<User> users = await this.LoadUsers(name);
            ViewData["name"] = name;

            return View(users.Where(u => u.Role != "Admin").OrderBy(u => u.FullName).ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> UsersAndRoles(string name)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            List<User> users = await this.LoadUsers(name);
            ViewData["name"] = name;

            return View(users.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal([Bind] User temp, string returnAction)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            User aux = (await this.LoadUsers(null)).FirstOrDefault(u => u.Email == temp?.Email);

            if (aux == null)
            {
                TempData["ErrorMessage"] = "No se encontró el usuario.";
                return RedirectToAction(returnAction ?? "Clients");
            }

            aux.FullName = temp.FullName;
            aux.PhoneNumber = temp.PhoneNumber;
            aux.Address = string.IsNullOrEmpty(temp.Address) ? aux.Address : temp.Address;
            aux.Role = string.IsNullOrEmpty(temp.Role) ? aux.Role : temp.Role;
            aux.Password = temp.Password;

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync<User>("/api/users/Update", aux);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                TempData["ErrorMessage"] = $"Error al modificar el usuario: {response.Content.ReadAsStringAsync().Result}";
            }

            return RedirectToAction(returnAction == "UsersAndRoles" ? "UsersAndRoles" : "Clients");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string email, string returnAction)
        {
            if (this.SetToken() == false)
            {
                return RedirectToAction("Login", "User");
            }

            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/users/Delete?email={Uri.EscapeDataString(email ?? "")}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                TempData["ErrorMessage"] = $"Error al eliminar el usuario: {response.Content.ReadAsStringAsync().Result}";
            }

            return RedirectToAction(returnAction == "UsersAndRoles" ? "UsersAndRoles" : "Clients");
        }

        private async Task<List<User>> LoadUsers(string name)
        {
            HttpResponseMessage response = null;

            if (string.IsNullOrEmpty(name))
            {
                response = await _httpClient.GetAsync("/api/users/List");
            }
            else
            {
                response = await _httpClient.GetAsync($"/api/users/SearchFullName?name={Uri.EscapeDataString(name)}");
            }

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<User>>(result) ?? new List<User>();
            }

            TempData["ErrorMessage"] = "No se pudieron cargar los usuarios.";
            return new List<User>();
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

        private async Task<User> SearchProfile(string email, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage http = await _httpClient.GetAsync("/api/users/Profile");

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
