using System.Text;
using System.Text.Json;

namespace MyNamespace
{
    public class Gemini
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public Gemini(
            string endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent",
            string apiKey = "AIzaSyB-x-I0olfsWFtaD_hT0h0fnwpc9IPIpLA")
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentNullException(nameof(endpoint));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            _endpoint = endpoint;
            _apiKey = apiKey;

            _httpClient = new HttpClient();
        }

        public async Task<string> SendRequestAsync(string systemPrompt, string userPrompt, int maxTokens = 256)
        {
            if (userPrompt == null) throw new ArgumentNullException(nameof(userPrompt));

            var payload = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\n{userPrompt}" }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = maxTokens
                }
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gemini uses ?key=API_KEY in the URL, not Authorization header
            string url = $"{_endpoint}?key={_apiKey}";
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            string responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini API request failed: {response.StatusCode} – {responseJson}");

            try
            {
                using JsonDocument doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                var text = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse Gemini API response: {ex.Message}. Raw response: {responseJson}");
            }
        }
    }
}
