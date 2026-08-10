using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmoozeshFront.Controllers
{
  //  [Authorize]
    public class PanelController : Controller
    {
        public IActionResult MainPanel()
        {
            ViewBag.UserName = User.Identity?.Name ?? "کاربر";
            ViewBag.Role = "مدیر ارشد";

            ViewBag.TotalUsers = 2456;
            ViewBag.NewStudents = 128;
            ViewBag.TotalCertificates = 42;
            ViewBag.SystemStatus = "95%";

            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }
    }
}