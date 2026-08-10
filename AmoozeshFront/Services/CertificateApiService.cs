// Services/CertificateApiService.cs
using System.Net.Http.Headers;
using System.Text.Json;
using AmoozeshFront.Models.Certificate;

namespace AmoozeshFront.Services
{
    public interface IAmoozeshService
    {
        Task<ApiListResponse<CertificateDto>> GetCertificatesByNationalCodeAsync(string nationalCode, string token);
        Task<ApiResponse<CertificateDto>> GetCertificateByIdAsync(long id, string token);
        Task<ApiResponse<CertificateDto>> GetCertificatePublicInfoAsync(long id);
    }

    public class CertificateApiService : IAmoozeshService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CertificateApiService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CertificateApiService(HttpClient httpClient, ILogger<CertificateApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiListResponse<CertificateDto>> GetCertificatesByNationalCodeAsync(string nationalCode, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Certificates/national/{nationalCode}");

                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("خطا در دریافت گواهینامه برای کد ملی {NationalCode}: {StatusCode}", nationalCode, response.StatusCode);
                    return new ApiListResponse<CertificateDto>
                    {
                        Success = false,
                        Message = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                            ? "نشست شما منقضی شده، دوباره وارد شوید"
                            : "خطا در دریافت اطلاعات از سرور"
                    };
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<ApiListResponse<CertificateDto>>(responseStream, _jsonOptions);
                return result ?? new ApiListResponse<CertificateDto> { Success = false, Message = "پاسخ نامعتبر از سرور" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت گواهینامه‌ها برای کد ملی {NationalCode}", nationalCode);
                return new ApiListResponse<CertificateDto>
                {
                    Success = false,
                    Message = "خطا در ارتباط با سرور"
                };
            }
        }

        public async Task<ApiResponse<CertificateDto>> GetCertificateByIdAsync(long id, string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/Certificates/{id}");

                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<CertificateDto>
                    {
                        Success = false,
                        Message = "گواهینامه یافت نشد"
                    };
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<ApiResponse<CertificateDto>>(responseStream, _jsonOptions);
                return result ?? new ApiResponse<CertificateDto> { Success = false, Message = "پاسخ نامعتبر از سرور" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت گواهینامه {Id}", id);
                return new ApiResponse<CertificateDto>
                {
                    Success = false,
                    Message = "خطا در ارتباط با سرور"
                };
            }
        }

        public async Task<ApiResponse<CertificateDto>> GetCertificatePublicInfoAsync(long id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Certificates/public/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<CertificateDto>
                    {
                        Success = false,
                        Message = "گواهینامه یافت نشد"
                    };
                }

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<ApiResponse<CertificateDto>>(responseStream, _jsonOptions);
                return result ?? new ApiResponse<CertificateDto> { Success = false, Message = "پاسخ نامعتبر از سرور" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اطلاعات عمومی گواهینامه {Id}", id);
                return new ApiResponse<CertificateDto>
                {
                    Success = false,
                    Message = "خطا در ارتباط با سرور"
                };
            }
        }
    }
}