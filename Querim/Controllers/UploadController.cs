using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Querim.Data;
using Querim.Dtos;
using Querim.Models;
using Querim.Services;
using System.Text.Json;

namespace Querim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly GeminiService _geminiService;
        private readonly ILogger<UploadController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public UploadController(GeminiService geminiService, ILogger<UploadController> logger, ApplicationDbContext dbContext)
        {
            _geminiService = geminiService;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost("chapters")]
        public async Task<IActionResult> CreateChapter([FromBody] ChapterCreateDto dto)
        {
            var chapter = new Chapter
            {
                Title = dto.Title,
                Description = dto.Description,
                PdfPath=dto.PdfPath,
                SubjectId = dto.SubjectId,
                 // assumes FilePath is part of DTO and Chapter model
            };

            _dbContext.Chapters.Add(chapter);
            await _dbContext.SaveChangesAsync();

            return Ok(chapter);
        }

        [HttpPost("upload/{chapterId}")]
        public async Task<IActionResult> UploadFileToChapter(int chapterId, IFormFile file)
        {
            // Validate chapter exists
            var chapter = await _dbContext.Chapters.FindAsync(chapterId);
            if (chapter == null)
            {
                return NotFound("Chapter not found");
            }

            // Validate file presence
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid file.");
            }

            // Validate file extension
            if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only .txt files are supported.");
            }

            string text;
            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                text = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file content");
                return BadRequest("Error reading file content.");
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("The file contains no text.");
            }

            List<GeminiService.QuizQuestion> questions;
            try
            {
                questions = await _geminiService.GenerateQuestionsAsync(text);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating questions");
                return StatusCode(500, "An error occurred while generating questions.");
            }

            var questionEntities = questions.Select(q => new QuizQuestionEntity
            {
                QuestionText = q.QuestionText,
                CorrectAnswer = q.CorrectAnswer,
                AnswersJson = JsonSerializer.Serialize(q.Answers),
                ChapterId = chapterId
            }).ToList();

            _dbContext.QuizQuestions.AddRange(questionEntities);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                count = questions.Count,
                questions
            });
        }
        [HttpGet("subjects/{subjectId}/chapters")]
        public async Task<IActionResult> GetChaptersBySubject(int subjectId)
        {
            var subject = await _dbContext.Subjects
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (subject == null)
                return NotFound($"Subject with ID {subjectId} not found.");

            var chapters = subject.Chapters.Select(c => new
            {
                c.Id,
                c.Title,
                c.Description,
                //c.SubjectId,
                SubjectName = subject.Title
                // Add other properties as needed
            }).ToList();

            return Ok(chapters);
        }
        [HttpGet("chapters/{chapterId}/questions")]
        public async Task<IActionResult> GetQuestionsByChapter(int chapterId)
        {
            var chapter = await _dbContext.Chapters
                .Include(c => c.QuizQuestions)
                .FirstOrDefaultAsync(c => c.Id == chapterId);

            if (chapter == null)
                return NotFound($"Chapter with ID {chapterId} not found.");

            var questions = chapter.QuizQuestions.Select(q => new
            {
                q.Id,
                q.QuestionText,
                q.CorrectAnswer,
                Answers = JsonSerializer.Deserialize<List<string>>(q.AnswersJson)
            }).ToList();

            return Ok(questions);
        }
    }
}