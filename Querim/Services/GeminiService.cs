using System.Text.Json;
using System.Text;

namespace Querim.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["GeminiApiKey"] ?? "YOUR-API-KEY"; 
        }
        public async Task<List<string>> GenerateQuestionsAsync(string text)
        {
            try
            {

                var endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
                var requestBody = new
                {
                    contents = new[]
                    {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $@" generate multiple-choice questions based on the {text}  

Format your response as a JSON array of objects where each object represents one question with the following structure:
[
    {{
        ""id"": [unique_number],
        ""question"": ""[the generated question]"",
        ""answers"": [""option1"", ""option2"", ""option3"", ""option4""],
        ""correct_answer"": ""[the correct option]""
    }},
    // more questions...
]

Requirements:
1. Create at least 10 questions covering different aspects of the document
2. Questions should test comprehension of important concepts
3. Make answers plausible but with one clearly correct option
4. Number the questions sequentially starting from 1
5. Only respond with the JSON array, no additional text or explanations
6. Ensure the JSON is valid and properly formatted

Please process the PDF I upload and respond with the questions in exactly this format. "}
                        }
                    }
                }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{endpoint}?key={_apiKey}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API error: {response.StatusCode} - {errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                return ParseGeminiResponse(responseString);
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging)
                Console.WriteLine($"Error calling Gemini API: {ex.Message}");
                throw; // Re-throw the exception to handle it in the controller
            }
        }

        private List<string> ParseGeminiResponse(string responseJson)
        {
            using var doc = JsonDocument.Parse(responseJson);
            var questions = new List<string>();

            var candidates = doc.RootElement.GetProperty("candidates");
            foreach (var candidate in candidates.EnumerateArray())
            {
                var content = candidate.GetProperty("content");
                var parts = content.GetProperty("parts");
                foreach (var part in parts.EnumerateArray())
                {
                    var text = part.GetProperty("text").GetString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        // Split response into individual questions
                        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .Select(q => q.Trim())
                            .Where(q => !string.IsNullOrEmpty(q))
                            .ToList();
                        questions.AddRange(lines);
                    }
                }
            }

            return questions;
        }
    }

}
