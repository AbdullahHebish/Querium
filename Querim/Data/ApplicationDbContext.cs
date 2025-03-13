using Microsoft.EntityFrameworkCore;
using Querim.Models;

namespace Querim.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Student> Students { get; set; }
    }

}
