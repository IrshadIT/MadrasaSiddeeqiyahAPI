using Microsoft.EntityFrameworkCore;
using MadrasaSiddeeqiyahAPI.Models;   // Must match Student.cs namespace

namespace MadrasaSiddeeqiyahAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
    }
}