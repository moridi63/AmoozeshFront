using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmoozeshFront.Controllers
{
  //  [Authorize]
    public class PanelController : Controller
    {
        // GET: Panel/MainPanel
        public IActionResult MainPanel()
        {
            ViewBag.UserName = User.Identity?.Name ?? "کاربر";
            ViewBag.Role = "مدیر ارشد";

            // می‌توانید اطلاعات آماری رو از دیتابیس یا API دریافت کنید
            ViewBag.TotalUsers = 2456;
            ViewBag.NewStudents = 128;
            ViewBag.TotalCertificates = 42;
            ViewBag.SystemStatus = "95%";

            return View();
        }

        // GET: Panel/Dashboard (نمایشگر کامل‌تر)
        public IActionResult Dashboard()
        {
            return View();
        }

        // GET: Panel/Profile (پروفایل کاربر)
        public IActionResult Profile()
        {
            return View();
        }

        // GET: Panel/Settings (تنظیمات)
        public IActionResult Settings()
        {
            return View();
        }

        // GET: Panel/Logout (خروج از سیستم)
        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}