using AmoozeshFront.Models.Certificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AmoozeshFront.Services;

namespace AmoozeshFront.Controllers // اصلاح نِیم‌اسپیس از Navahi به نام اصلی پروژه
{
    [Authorize]
    [Route("Certificates/[action]")] // اضافه شدن روت برای هماهنگی با آدرس مرورگر (با s)
    public class CertificateController : Controller
    {
        private readonly IAmoozeshService _amoozeshService;

        public CertificateController(IAmoozeshService amoozeshService)
        {
            _amoozeshService = amoozeshService;
        }

        [HttpGet]
        [Route("/Certificates/Search")] // روت صریح برای متد گت
        public IActionResult Search()
        {
            var model = new CertificateSearchViewModel
            {
                Results = new List<CertificateDto>(),
                HasSearched = false
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(CertificateSearchViewModel model)
        {
            model.Results ??= new List<CertificateDto>();

            if (!ModelState.IsValid)
            {
                model.HasSearched = false;
                return View(model);
            }

            var token = User.FindFirst("JwtToken")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _amoozeshService.GetCertificatesByNationalCodeAsync(model.NationalCode, token);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message ?? "خطا در دریافت اطلاعات از سامانه آموزش.");
                    model.HasSearched = false;
                    return View(model);
                }

                model.Results = result.Data ?? new List<CertificateDto>();
                model.HasSearched = true;
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "بروز خطای غیرمنتظره در برقراری ارتباط با سرور آموزش.");
                model.HasSearched = false;
            }

            return View(model);
        }
    }
}