// Services/IQRCodeService.cs
using AmoozeshFront.Models.Certificate;

namespace AmoozeshFront.Services
{
    public interface IQRCodeService
    {
        string GenerateQRCode(string data);
        string GenerateCertificateQRCode(CertificateData data);
    }
}