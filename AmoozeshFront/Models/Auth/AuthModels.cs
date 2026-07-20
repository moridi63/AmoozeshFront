using System.ComponentModel.DataAnnotations;
using AmoozeshFront.Core.Enums;

namespace AmoozeshFront.Models.Auth
{
    // مدل فرم ورود در سمت فرانت
    public class LoginViewModel
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [Display(Name = "مرا به خاطر بسپار")]
        public bool RememberMe { get; set; }
    }

    // معادل LoginDto سمت بک‌اند - برای ارسال به API
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // معادل LoginResponseDto سمت بک‌اند - برای دریافت از API
    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string PersonnelCode { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiry { get; set; }
        public bool IsActive { get; set; }
    }

    // رَپر عمومی برای پاسخ‌های API طبق فرمت { success, message, data }
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}