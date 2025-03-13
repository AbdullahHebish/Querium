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

        [HttpPost("approve-student/{id}")]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            student.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Student approved" });
        }
    }
}
