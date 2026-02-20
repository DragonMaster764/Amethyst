using Amethyst.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Amethyst.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {

        public DbSet<Reminder> Reminders { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<StudySession> StudySessions { get; set; }

        public DbSet<Tasks> Tasks { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Reminder table configuration
            modelBuilder.Entity<Reminder>()
            .HasCheckConstraint("CK_Reminder_TargetType",
             "(target_type = 'Assignment' AND assignment_id IS NOT NULL AND task_id IS NULL) OR " +
             "(target_type = 'Task' AND task_id IS NOT NULL AND assignment_id IS NULL)");

            modelBuilder.Entity<Student>()
            .HasCheckConstraint("CK_Student_Type",
            "student_type IN ('High School', 'College')");

            modelBuilder.Entity<Student>()
            .HasCheckConstraint("CK_Student_Grad_Year",
             "graduation_year BETWEEN 1900 AND 3000");

            modelBuilder.Entity<StudySession>()
            .HasCheckConstraint("CK_End_Time",
            "end_time IS NULL OR end_time >= start_time");

            modelBuilder.Entity<StudySession>()
            .HasCheckConstraint("CK_Planned_Min",
            "planned_minutes IS NULL OR planned_minutes BETWEEN 0 AND 360");

            modelBuilder.Entity<StudySession>()
            .HasCheckConstraint("CK_Actual_Min",
            "actual_minutes IS NULL OR actual_minutes BETWEEN 0 AND 360");


            //Will be added if Course PK is composite
            //modelBuilder.Entity<StudySession>()
            //    .HasOne(s => s.Course)
            //    .WithMany(c => c.StudySessions)
            //    .HasForeignKey(s => new { s.CourseId, s.ProfileId });

            modelBuilder.Entity<Tasks>()
    .       HasCheckConstraint("CK_Task_Status",
            "status IN ('In Progress', 'Not Started', 'Completed')");

            modelBuilder.Entity<Tasks>()
            .HasCheckConstraint("CK_Task_Priority",
            "priority IN ('Low', 'Medium', 'High')");

            modelBuilder.Entity<Tasks>()
            .HasCheckConstraint("CK_Estimated_Min",
            "estimated_minutes >= 0 AND estimated_minutes <= 10000");


        }
    }
}
