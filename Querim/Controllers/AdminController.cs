using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Querim.Data;
using Querim.Dtos;
using Querim.Models;
using System.Threading.Tasks;

namespace Querim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto loginDto)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == loginDto.Email);

            if (admin == null || admin.Password != loginDto.Password)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            return Ok(new
            {
                message = "Login successful",
                admin = new
                {
                    admin.Email,
                    admin.Password
                }
            });
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {

            return Ok(new { message = "Admin logout successful" });
        }


        [HttpPost("approve-student/{universityIdCard}")]
        public async Task<IActionResult> ApproveStudent(string universityIdCard)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UniversityIDCard == universityIdCard);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            student.Status = "Approved";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Student approved" });
        }

        [HttpPost("reject-student/{universityIdCard}")]
        public async Task<IActionResult> RejectStudent(string universityIdCard)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UniversityIDCard == universityIdCard);

            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }
            student.IsDeleted = true;
            student.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Student rejected" });
        }

        [HttpGet("students")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            var students = await _context.Students
                .Where(s => !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.Email,
                    s.UniversityIDCard,
                    s.NationalIDCard,
                    s.Status,
                    s.CreatedAt
                })
                .ToListAsync();

            return Ok(students);
        }
        [HttpPost("upload-subject")]
        public async Task<IActionResult> UploadSubject([FromBody] SubjectUploadDto subjectDto)
        {
            var subject = new Subject
            {
                Title = subjectDto.Title,
                Description = subjectDto.Description,
                AcademicYear = subjectDto.AcademicYear,
                Semester = subjectDto.Semester,
                Chapters = subjectDto.Chapters.Select(c => new Chapter
                {
                    Title = c.Title,
                    Description = c.Description,
                    PdfPath = c.PdfPath
                }).ToList()
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Subject uploaded successfully" });
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjects()
        {
            var subjects = await _context.Subjects
                .Include(s => s.Chapters)
                .ThenInclude(c => c.Questions)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Description,
                    s.AcademicYear,
                    s.Semester,
                    Chapters = s.Chapters.Select(c => new
                    {
                        c.Id,
                        c.Title,
                        c.Description,
                        c.PdfPath,
                        Questions = c.Questions.Select(q => new
                        {
                            q.Id,
                            q.Text,
                            q.Answer
                        })
                    })
                })
                .ToListAsync();

            return Ok(subjects);
        }
        //[HttpPost("approve-student/{id}")]
        //public async Task<IActionResult> ApproveStudent(int id)
        //{
        //    var student = await _context.Students.FindAsync(id);
        //    if (student == null)
        //    {
        //        return NotFound(new { message = "Student not found" });
        //    }

        //    student.IsApproved = true;
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Student approved" });
        //}
        //[HttpPost("approve-student/{universityIDCard}")]
        //public async Task<IActionResult> ApproveStudent(string universityIDCard)
        //{
        //    var student = await _context.Students
        //        .FirstOrDefaultAsync(s => s.UniversityIDCard == universityIDCard);

        //    if (student == null)
        //    {
        //        return NotFound(new { message = "Student not found" });
        //    }

        //    student.IsApproved = true;
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Student approved" });
        //}
    }
}
