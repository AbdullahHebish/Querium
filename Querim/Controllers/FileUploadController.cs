using Microsoft.AspNetCore.Mvc;
using Querim.Services;

namespace Querim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly GeminiService _geminiService;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(
            GeminiService geminiService,
            ILogger<FileUploadController> logger)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file or empty file uploaded");
                    return BadRequest(new { error = "Please upload a valid file" });
                }

                if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"Invalid file type uploaded: {file.FileName}");
                    return BadRequest(new { error = "Only .txt files are supported" });
                }

                // Read file content
                string text;
                try
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    text = await reader.ReadToEndAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading file content");
                    return BadRequest(new { error = "Error reading file content" });
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Empty text content in file");
                    return BadRequest(new { error = "The file contains no text" });
                }

                // Generate questions
                try
                {
                    _logger.LogInformation("Generating questions from text content");
                    var questions = await _geminiService.GenerateQuestionsAsync(text);
                    _logger.LogInformation($"Successfully generated {questions.Count} questions");

                    return Ok(new
                    {
                        success = true,
                        count = questions.Count,
                        questions = questions
                    });
                }
                catch (HttpRequestException httpEx)
                {
                    _logger.LogError(httpEx, "Gemini API request failed");
                    return StatusCode(502, new
                    {
                        error = "Error communicating with Gemini API",
                        details = httpEx.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing file upload");
                return Problem(
                    title: "Error processing file",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        }
    }
}