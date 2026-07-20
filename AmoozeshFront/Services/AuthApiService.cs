using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AmoozeshFront.Models.Auth;

namespace AmoozeshFront.Services
{
    public interface IAuthApiService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    }

    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthApiService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthApiService(HttpClient httpClient, ILogger<AuthApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Auth/login", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            var apiResult = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(responseBody, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ورود ناموفق: {Message}", apiResult?.Message);
                return apiResult ?? new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "خطا در ارتباط با سرور"
                };
            }

            return apiResult;
        }
    }
}