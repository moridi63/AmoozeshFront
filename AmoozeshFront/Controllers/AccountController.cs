using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AmoozeshFront.Models.Auth;
using AmoozeshFront.Services;

namespace AmoozeshFront.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthApiService _authApiService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthApiService authApiService, ILogger<AccountController> logger)
        {
            _authApiService = authApiService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _authApiService.LoginAsync(new LoginRequestDto
            {
                Username = model.Username,
                Password = model.Password
            });

            if (result == null || !result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "نام کاربری یا رمز عبور اشتباه است");
                return View(model);
            }

            var userData = result.Data;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userData.Id.ToString()),
                new Claim(ClaimTypes.Name, userData.Username),
                new Claim("FullName", userData.FullName),
                new Claim("PersonnelCode", userData.PersonnelCode ?? ""),
                new Claim(ClaimTypes.Role, userData.AccessLevel.ToString()),
                new Claim("JwtToken", userData.Token) // برای فراخوانی بعدی APIهای محافظت‌شده
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = userData.TokenExpiry
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("کاربر {Username} با موفقیت وارد شد", userData.Username);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            
            return RedirectToAction("MainPanel", "Panel");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}