using Amethyst.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Amethyst.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Course> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Assignment>(entity =>
            {
                // 1. Map to Table Name
                entity.ToTable("Assignment");

                // 2. Configure Decimal Precision
                entity.Property(e => e.Points).HasPrecision(6, 2);
                entity.Property(e => e.TotalPoints).HasPrecision(6, 2);
                entity.Property(e => e.RawPercentage).HasPrecision(5, 2);

                // 3. SQL Check Constraints (The 'Rules' from your SQL)
                entity.HasCheckConstraint("CK_Assignment_Status", "status IN ('In Progress', 'Not Started', 'Completed')");
                entity.HasCheckConstraint("CK_Assignment_Priority", "priority IN ('Low', 'Medium', 'High')");
                entity.HasCheckConstraint("CK_Est_Min", "estimated_minutes >= 0 AND estimated_minutes <= 10000");

                // 4. Relationships (Foreign Keys)
                entity.HasOne(d => d.Course)
                      .WithMany(p => p.Assignments)
                      .HasForeignKey(d => d.CourseId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Assignment_Course");
            });
        }
    }
}
