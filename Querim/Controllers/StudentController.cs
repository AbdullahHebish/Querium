
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
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] StudentRegisterDto registerDto)
        {
            if (await _context.Students.AnyAsync(s => s.Email == registerDto.Email||s.NationalIDCard==registerDto.NationalIDCard||s.UniversityIDCard==registerDto.UniversityIDCard))
            {
                return BadRequest(new { message = "already exists" });
            }

            var student = new Student
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PasswordHash = registerDto.Password, // Use hashing in production
                UniversityIDCard = registerDto.UniversityIDCard,
                NationalIDCard = registerDto.NationalIDCard
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful, pending approval",
                student = new
                {
                    student.Id,
                    student.FullName,
                    student.Email,
                    student.UniversityIDCard,
                    student.NationalIDCard,
                    student.IsApproved,
                    student.IsDeleted,
                    student.CreatedAt
                }
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] StudentLoginDto loginDto)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == loginDto.Email && !s.IsDeleted);

            if (student == null || student.PasswordHash != loginDto.Password)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (!student.IsApproved)
            {
                return Unauthorized(new { message = "Account not approved" });
            }

            // return Ok(new { message = "Login successful" });
            return Ok(new
            {
                message = "Login successful",
                student = new
                {
                    student.Id,
                    student.FullName,
                    student.Email,
                    student.UniversityIDCard,
                    student.NationalIDCard,
                    student.IsApproved,
                    student.IsDeleted,
                    student.CreatedAt
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Implement logout logic (e.g., clear session)
            return Ok(new { message = "Logout successful" });
        }
    }
}
