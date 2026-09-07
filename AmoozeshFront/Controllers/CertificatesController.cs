using AmoozeshFront.Models.Certificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AmoozeshFront.Services;
using System.Linq;

namespace AmoozeshFront.Controllers
{
    [Authorize]
    [Route("Certificates/[action]")]
    public class CertificateController : Controller
    {
        private readonly IAmoozeshService _amoozeshService;
        private readonly IQRCodeService _qrCodeService;

        public CertificateController(IAmoozeshService amoozeshService, IQRCodeService qrCodeService)
        {
            _amoozeshService = amoozeshService;
            _qrCodeService = qrCodeService;
        }

        [HttpGet]
        [Route("/Certificates/Search")]
        public IActionResult Search()
        {
            var model = new CertificateSearchViewModel
            {
                Results = new List<CertificateDto>(),
                AllCertificates = new List<CertificateDto>(),
                HasSearched = false
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(CertificateSearchViewModel model)
        {
            model.Results ??= new List<CertificateDto>();
            model.AllCertificates ??= new List<CertificateDto>();

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
                // ۱. دریافت نتایج جستجو بر اساس کد ملی
                var result = await _amoozeshService.GetCertificatesByNationalCodeAsync(model.NationalCode, token);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message ?? "خطا در دریافت اطلاعات از سامانه آموزش.");
                    model.HasSearched = false;
                    return View(model);
                }

                var certificates = result.Data ?? new List<CertificateDto>();

                // تولید QR Code برای نتایج جستجو
                foreach (var cert in certificates)
                {
                    cert.QRCodeBase64 = _qrCodeService.GenerateCertificateQRCode(MapToCertificateData(cert));
                }

                model.Results = certificates;

                // ۲. دریافت کل گواهینامه‌های دیتابیس برای «چاپ همه»
                var allResult = await _amoozeshService.GetAllCertificatesAsync(token);
                if (allResult.Success && allResult.Data != null)
                {
                    foreach (var cert in allResult.Data)
                    {
                        cert.QRCodeBase64 = _qrCodeService.GenerateCertificateQRCode(MapToCertificateData(cert));
                    }
                    model.AllCertificates = allResult.Data;
                }

                model.HasSearched = true;
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "بروز خطای غیرمنتظره در برقراری ارتباط با سرور آموزش.");
                model.HasSearched = false;
            }

            return View(model);
        }

        /// <summary>
        /// دریافت تمام گواهینامه‌های ثبت‌شده در دیتابیس بدون نیاز به کد ملی
        /// برای استفاده در قابلیت «چاپ تمام گواهینامه‌های سیستم»
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllForPrint()
        {
            var token = User.FindFirst("JwtToken")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, message = "ابتدا وارد سیستم شوید." });
            }

            try
            {
                var allResult = await _amoozeshService.GetAllCertificatesAsync(token);

                if (!allResult.Success || allResult.Data == null)
                {
                    return Json(new { success = false, message = allResult.Message ?? "خطا در دریافت اطلاعات از سامانه آموزش." });
                }

                var data = allResult.Data.Select(c => new
                {
                    id = c.Id,
                    fullName = c.FullName ?? "",
                    fatherName = c.FatherName ?? "",
                    nationalCode = c.NationalCode ?? "",
                    companyName = c.CompanyName ?? "",
                    examTitle = c.ExamTitle ?? "",
                    examDate = c.ExamDate.HasValue ? c.ExamDate.Value.ToString("yyyy/MM/dd") : (c.ExamDatePersian ?? ""),
                    certNumber = c.CertificateNumber ?? "",
                    qrCodeBase64 = _qrCodeService.GenerateCertificateQRCode(MapToCertificateData(c))
                }).ToList();

                return Json(new { success = true, data });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "خطای غیرمنتظره در دریافت گواهینامه‌ها." });
            }
        }

        [AllowAnonymous]
        [HttpGet("/Certificate/Verify/{id:long}")]
        public async Task<IActionResult> Verify(long id)
        {
            try
            {
                var token = User.FindFirst("JwtToken")?.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    var result = await _amoozeshService.GetCertificateByIdAsync(id, token);
                    if (result.Success && result.Data != null)
                    {
                        result.Data.QRCodeBase64 = _qrCodeService.GenerateCertificateQRCode(MapToCertificateData(result.Data));
                        return View("Verify", result.Data);
                    }
                }

                var publicResult = await _amoozeshService.GetCertificatePublicInfoAsync(id);
                if (!publicResult.Success || publicResult.Data == null)
                {
                    return NotFound("گواهینامه مورد نظر یافت نشد.");
                }

                publicResult.Data.QRCodeBase64 = _qrCodeService.GenerateCertificateQRCode(MapToCertificateData(publicResult.Data));

                return View("Verify", publicResult.Data);
            }
            catch (Exception)
            {
                return View("Error", new { message = "خطا در تایید گواهینامه" });
            }
        }

        // متد کمکی برای نگاشت و جلوگیری از تکرار کد
        private static CertificateData MapToCertificateData(CertificateDto cert)
        {
            return new CertificateData
            {
                Id = cert.Id,
                CertificateNumber = cert.CertificateNumber,
                FullName = cert.FullName,
                NationalCode = cert.NationalCode,
                ExamTitle = cert.ExamTitle,
                ExamDate = cert.ExamDate,
                CompanyName = cert.CompanyName,
                FatherName = cert.FatherName,
                Gender = cert.Gender,
                Status = cert.StatusDisplay
            };
        }
    }
}
