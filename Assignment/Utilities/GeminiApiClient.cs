using Assignment.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace Assignment.Utilities
{
    public class GeminiApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _modelName;

        public GeminiApiClient(IOptions<GeminiSetting> settings)
        {
            _httpClient = new HttpClient();
            _apiKey = settings.Value.ApiKey;
            _modelName = settings.Value.ModelName;
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            // Đảm bảo model name đúng format, ví dụ: gemini-1.5-flash, gemini-1.5-pro
            string normalizedModelName = _modelName.StartsWith("models/") ? _modelName : $"models/{_modelName}";

            // URL API chính xác cho Gemini
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/{normalizedModelName}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

                // Log URL và response để debug
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    return $"API Error ({response.StatusCode}): {errorContent}. URL: {apiUrl}";
                }

                string responseString = await response.Content.ReadAsStringAsync();
                dynamic responseObject = JsonConvert.DeserializeObject(responseString);

                if (responseObject?.candidates != null && responseObject.candidates.Count > 0 &&
                    responseObject.candidates[0]?.content?.parts != null && responseObject.candidates[0].content.parts.Count > 0)
                {
                    return responseObject.candidates[0].content.parts[0].text;
                }

                return "Không thể trích xuất nội dung từ phản hồi của Gemini.";
            }
            catch (HttpRequestException ex)
            {
                return $"Lỗi kết nối hoặc HTTP: {ex.Message}";
            }
            catch (JsonException ex)
            {
                return $"Lỗi phân tích JSON: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Đã xảy ra lỗi không mong muốn: {ex.Message}";
            }
        }

        // Method để dispose HttpClient khi không cần thiết
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}