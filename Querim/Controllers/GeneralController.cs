using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Querim.Data;
using Querim.Dtos;
using Querim.Models;

namespace Querim.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GeneralController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjectsByAcademicYear([FromQuery] int? academicYear)
        {
            IQueryable<Subject> query = _context.Subjects;

            if (academicYear.HasValue)
            {
                query = query.Where(s => s.AcademicYear == academicYear.Value);
            }

            var subjects = await query
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Description,
                    s.AcademicYear,
                    s.Semester
                })
                .ToListAsync();

            return Ok(subjects);
        }

        [HttpGet("subjects/search")]
        public async Task<IActionResult> SearchSubjects([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "Search query cannot be empty." });
            }

            query = query.Trim().ToLower();

            var filteredSubjects = await _context.Subjects
                .Where(s => s.Title.ToLower().Contains(query) ||
                            (s.Description != null && s.Description.ToLower().Contains(query)))
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Description,
                    s.AcademicYear,
                    s.Semester
                })
                .ToListAsync();

            return Ok(filteredSubjects);
        }
      
    }
}
