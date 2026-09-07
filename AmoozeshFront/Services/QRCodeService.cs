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
            // تغییر محتوای QR Code به آدرس لاگین
            var loginUrl = "https://ibtc.ir";

            // فقط آدرس لاگین را برگردان (به جای JSON)
            return GenerateQRCode(loginUrl);
        }
    }
}