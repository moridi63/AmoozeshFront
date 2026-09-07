using AmoozeshFront.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AmoozeshFront.Models.Certificate
{
    public class CertificateDto
    {
        public long Id { get; set; }
        public string CertificateNumber { get; set; }
        public bool Gender { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        public string NationalCode { get; set; }
        public string CompanyName { get; set; }
        public string ExamTitle { get; set; }
        public DateTime? ExamDate { get; set; }
        public int? DurationHours { get; set; }
        public CertificateStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public string Barcode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string GenderDisplay => Gender ? "زن" : "مرد";
        public string QRCodeBase64 { get; set; }
        public string? ExamDatePersian { get; set; }
    }

    public class CertificateSearchViewModel
    {
        [Required(ErrorMessage = "کد ملی الزامی است")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "کد ملی باید 10 رقم باشد")]
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        public List<CertificateDto> Results { get; set; } = new();
        public List<CertificateDto> AllCertificates { get; set; } = new(); // نتایج کل سیستم برای چاپ یکجا
        public bool HasSearched { get; set; } = false;
    }

    public class ApiListResponse<T>
    {
        public bool Success { get; set; }
        public List<T> Data { get; set; } = new();
        public int Count { get; set; }
        public string Message { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }

    public class CertificateData
    {
        public long Id { get; set; }
        public string CertificateNumber { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string ExamTitle { get; set; }
        public DateTime? ExamDate { get; set; }
        public string CompanyName { get; set; }
        public string FatherName { get; set; }
        public bool Gender { get; set; }
        public string Status { get; set; }
    }
}