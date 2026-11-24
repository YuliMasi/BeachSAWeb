using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
    config =>
    {
        config.Cookie.Name = "UserLoginCookie";
        config.LoginPath = "/User/Login";
        config.Cookie.HttpOnly = true;
        config.ExpireTimeSpan = TimeSpan.FromMinutes(8);
        config.AccessDeniedPath = "/";
        config.SlidingExpiration = false;
    }
    );

builder.Services.AddSession(
    option =>
    {
        option.IdleTimeout = TimeSpan.FromMinutes(8);
        option.Cookie.HttpOnly = true;
        option.Cookie.IsEssential = true;
    }
    );

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();