// Controllers/CertificateController.cs
using AmoozeshFront.Models.Certificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AmoozeshFront.Services;

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

                var certificates = result.Data ?? new List<CertificateDto>();

                // تولید QR Code برای هر گواهینامه
                foreach (var cert in certificates)
                {
                    var certificateData = new CertificateData
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

                    cert.QRCodeBase64 = _qrCodeService.GenerateCertificateQRCode(certificateData);
                }

                model.Results = certificates;
                model.HasSearched = true;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "بروز خطای غیرمنتظره در برقراری ارتباط با سرور آموزش.");
                model.HasSearched = false;
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet("/Certificate/Verify/{id:long}")]
        public async Task<IActionResult> Verify(long id)
        {
            try
            {
                var token = User.FindFirst("JwtToken")?.Value;

                // اگر کاربر لاگین است از اطلاعات کامل استفاده کن
                if (!string.IsNullOrEmpty(token))
                {
                    var result = await _amoozeshService.GetCertificateByIdAsync(id, token);
                    if (result.Success && result.Data != null)
                    {
                        // تولید QR Code برای نمایش
                        var certData = new CertificateData
                        {
                            Id = result.Data.Id,
                            CertificateNumber = result.Data.CertificateNumber,
                            FullName = result.Data.FullName,
                            NationalCode = result.Data.NationalCode,
                            ExamTitle = result.Data.ExamTitle,
                            ExamDate = result.Data.ExamDate,
                            CompanyName = result.Data.CompanyName,
                            FatherName = result.Data.FatherName,
                            Gender = result.Data.Gender,
                            Status = result.Data.StatusDisplay
                        };
                        result.Data.QRCodeBase64 = _qrCodeService.GenerateCertificateQRCode(certData);
                        return View("Verify", result.Data);
                    }
                }

                // اگر لاگین نیست اطلاعات عمومی را نمایش بده
                var publicResult = await _amoozeshService.GetCertificatePublicInfoAsync(id);
                if (!publicResult.Success || publicResult.Data == null)
                {
                    return NotFound("گواهینامه مورد نظر یافت نشد.");
                }

                // تولید QR Code برای نمایش عمومی
                var publicCertData = new CertificateData
                {
                    Id = publicResult.Data.Id,
                    CertificateNumber = publicResult.Data.CertificateNumber,
                    FullName = publicResult.Data.FullName,
                    NationalCode = publicResult.Data.NationalCode,
                    ExamTitle = publicResult.Data.ExamTitle,
                    ExamDate = publicResult.Data.ExamDate,
                    CompanyName = publicResult.Data.CompanyName,
                    FatherName = publicResult.Data.FatherName,
                    Gender = publicResult.Data.Gender,
                    Status = publicResult.Data.StatusDisplay
                };
                publicResult.Data.QRCodeBase64 = _qrCodeService.GenerateCertificateQRCode(publicCertData);

                return View("Verify", publicResult.Data);
            }
            catch (Exception ex)
            {
                return View("Error", new { message = "خطا در تایید گواهینامه" });
            }
        }
    }
}