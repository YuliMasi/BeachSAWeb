using BeachSAWeb.Models;
using BeachSAWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] User temp)
        {
            AuthorizationResponse authorization = null;

            if (temp == null)
            {
                TempData["ErrorMessage"] = "No hay datos para la autenticación";
                return View(temp);
            }
            else
            {
                HttpResponseMessage http = await
                    _httpClient.PostAsJsonAsync<User>("/api/users/AutenticacionUsuario", temp);

                if (http.IsSuccessStatusCode)
                {
                    var result = http.Content.ReadAsStringAsync().Result;
                    authorization = JsonConvert.DeserializeObject<AuthorizationResponse>(result);

                    if (authorization != null && authorization.Result == true)
                    {
                        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                        identity.AddClaim(new Claim(ClaimTypes.Name, temp.Email));
                        var mainSession = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme, mainSession);
                        HttpContext.Session.SetString("token", authorization.Token);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Usuario o password incorrecto..";
                        return View(temp);
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Usuario o password incorrecto..";
                    return View(temp);
                }
            }

        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "User");
        }

    }
}