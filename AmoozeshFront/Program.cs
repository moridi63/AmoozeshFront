using AmoozeshFront.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// تنظیمات Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// اضافه کردن Services
builder.Services.AddControllersWithViews();

// ۱. ماندگاری کلیدهای رمزنگاری برای جلوگیری از پریدن سشن کاربر پس از ریستارت سرور
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "temp-keys");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("AmoozeshFrontApp");

// تنظیم HttpClient برای AuthApiService
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
    }
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// تنظیم HttpClient برای CertificateApiService
builder.Services.AddHttpClient<IAmoozeshService, CertificateApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
    }
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// تنظیمات Authentication (سازگار با سرورهای بدون HTTPS)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // اصلاح اصلی: کوکی با پروتکل درخواست (HTTP یا HTTPS) هماهنگ می‌شود
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.Cookie.Name = "AmoozeshFront.Auth";
        options.ReturnUrlParameter = "returnUrl";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// تنظیمات Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ۲. غیرفعال‌سازی ریدایرکت خودکار به HTTPS به علت نداشتن گواهی SSL روی سرور
// app.UseHttpsRedirection(); 

app.UseStaticFiles();
app.UseRouting();

// ترتیب Middlewareها بسیار مهم است
app.UseAuthentication();
app.UseAuthorization();

// Middleware برای ریدایرکت ریشه به Login
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    await next();
});

// ۳. حذف Middleware دستی احراز هویت (کنترل دسترسی‌ها توسط [Authorize] در کنترلر انجام می‌شود)

// تنظیمات Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// مسیرهای خاص برای Certificate
app.MapControllerRoute(
    name: "certificates",
    pattern: "Certificates/{action=Search}/{id?}",
    defaults: new { controller = "Certificate" });

// مسیر تست
app.MapGet("/test", () => "Server is running!");

app.Run();