// Services/QRCodeService.cs
using AmoozeshFront.Models.Certificate;
using QRCoder;
using System;
using System.Text.Json;

namespace AmoozeshFront.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly IConfiguration _configuration;

        public QRCodeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateQRCode(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            var qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }

        public string GenerateCertificateQRCode(CertificateData data)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://certificate.ibtc.ir";

            var qrData = new
            {
                id = data.Id,
                certificateNumber = data.CertificateNumber,
                fullName = data.FullName,
                nationalCode = data.NationalCode,
                examTitle = data.ExamTitle,
                examDate = data.ExamDate?.ToString("yyyy-MM-dd"),
                companyName = data.CompanyName,
                fatherName = data.FatherName,
                gender = data.Gender,
                status = data.Status,
                verificationUrl = $"{baseUrl}/Certificate/Verify/{data.Id}"
            };

            var jsonData = JsonSerializer.Serialize(qrData);
            return GenerateQRCode(jsonData);
        }
    }
}