using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // التنفيذ الوحيد الذي "يعرف" تفاصيل Gemini API الفعلية (شكل الطلب، شكل الاستجابة،
    // مسار key/model). مسجَّل كـTyped HttpClient عبر IHttpClientFactory في Program.cs
    // (بدلاً من "new HttpClient()" المتكرر الذي كان موجودًا سابقًا داخل ChatService).
    public class GeminiClient : IGeminiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly ILogger<GeminiClient> _logger;

        public GeminiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiClient> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
            _apiUrl = configuration["Gemini:ApiUrl"];
            _logger = logger;

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl))
                throw new InvalidOperationException("Gemini API key or URL is missing in configuration.");
        }

        public async Task<GeminiReplyResult> GenerateReplyAsync(
            string systemPrompt,
            List<GeminiHistoryMessage> history,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response;
            string responseJson;

            try
            {
                var contents = history.Select(msg => new
                {
                    role = msg.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = msg.Content } }
                }).ToList<object>();

                var requestBody = new
                {
                    system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                    contents
                };

                var json = JsonSerializer.Serialize(requestBody);
                using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                // الـKey لا يُسجَّل أبدًا في أي Log — فقط يُستخدَم في الرابط الفعلي للاتصال.
                response = await _httpClient.PostAsync($"{_apiUrl}?key={_apiKey}", httpContent, cancellationToken);
                responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // الطلب انتهى بسبب انتهاء مهلة HttpClient.Timeout المُعرَّفة في Program.cs، لا إلغاء المستخدم.
                _logger.LogWarning("Gemini API request timed out.");
                return GeminiReplyResult.Fail("timeout");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Gemini API request failed due to a network error.");
                return GeminiReplyResult.Fail("network_error");
            }

            if (!response.IsSuccessStatusCode)
            {
                // لا يتم تسجيل محتوى الاستجابة نفسه (قد يحتوي رسالة خطأ من المزوّد)، فقط الكود.
                _logger.LogWarning("Gemini API returned non-success status {StatusCode}.", response.StatusCode);
                return GeminiReplyResult.Fail("provider_error");
            }

            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return string.IsNullOrEmpty(text)
                    ? GeminiReplyResult.Fail("empty_response")
                    : GeminiReplyResult.Ok(text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini API response.");
                return GeminiReplyResult.Fail("invalid_response_format");
            }
        }
    }

}
