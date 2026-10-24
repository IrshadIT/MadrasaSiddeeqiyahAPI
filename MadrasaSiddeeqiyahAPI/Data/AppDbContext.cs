using Microsoft.EntityFrameworkCore;
using MadrasaSiddeeqiyahAPI.Models;

namespace MadrasaSiddeeqiyahAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Campus> Campuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure usernames are unique.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure monetary fields with 2 decimal places.
            modelBuilder.Entity<Campus>(entity =>
            {
                entity.Property(c => c.AnnualBudget)
                    .HasPrecision(18, 2);

                entity.Property(c => c.TotalSpent)
                    .HasPrecision(18, 2);

                entity.Property(c => c.TotalReceived)
                    .HasPrecision(18, 2);
            });

            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Donation>()
                .Property(d => d.Amount)
                .HasPrecision(18, 2);
        }
    }
}
