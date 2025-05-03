using Microsoft.AspNetCore.Mvc;
using Querim.Services;

namespace Querim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public FileUploadController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file == null || !file.FileName.EndsWith(".txt"))
                    return BadRequest(new { error = "Please upload a .txt file" });

                if (file.Length == 0)
                    return BadRequest(new { error = "The file is empty" });

                using var reader = new StreamReader(file.OpenReadStream());
                var text = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(text))
                    return BadRequest(new { error = "The file contains no text" });

                var questions = await _geminiService.GenerateQuestionsAsync(text);
                return Ok(new { Questions = questions });
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Error processing file",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        }
    }
}
