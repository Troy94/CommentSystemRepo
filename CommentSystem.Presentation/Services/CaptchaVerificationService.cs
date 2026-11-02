using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CommentSystem.Presentation.Services
{
    public interface ICaptchaVerificationService
    {
        Task<bool> VerifyAsync(string? captchaToken, CancellationToken cancellationToken = default);
    }

    public class CaptchaVerificationService : ICaptchaVerificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CaptchaVerificationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> VerifyAsync(string? captchaToken, CancellationToken cancellationToken = default)
        {
            var isEnabled = _configuration.GetValue<bool>("Captcha:Enabled", false);
            if (!isEnabled)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(captchaToken))
            {
                return false;
            }

            var secretKey = _configuration["Captcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return false;
            }

            using var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = secretKey,
                ["response"] = captchaToken
            });

            using var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", requestContent, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var verificationResult = await response.Content.ReadFromJsonAsync<CaptchaVerificationResponse>(cancellationToken: cancellationToken);
            return verificationResult?.Success == true;
        }

        private sealed class CaptchaVerificationResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }
    }
}
