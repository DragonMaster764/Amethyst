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

        public DbSet<UserTask> TaskItems { get; set; }

        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Course> Profile { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Profile>().Ignore(p => p.User);
            modelBuilder.Entity<UserTask>().ToTable("Tasks");
            modelBuilder.Entity<Profile>().ToTable("Profile");

            modelBuilder.Entity<UserTask>()
                .HasOne(t => t.Profile)
                .WithMany()
                .HasForeignKey(t => t.ProfileId)
                .HasPrincipalKey(p => p.ProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Profile>(p => p.ProfileId)
                .HasPrincipalKey<IdentityUser>(u => u.Id)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<UserTask>().ToTable("Tasks");

            modelBuilder.Entity<UserTask>()
    .       HasCheckConstraint("CK_Task_Status",
            "status IN ('In Progress', 'Not Started', 'Completed')");

            modelBuilder.Entity<UserTask>()
            .HasCheckConstraint("CK_Task_Priority",
            "priority IN ('Low', 'Medium', 'High')");

            modelBuilder.Entity<UserTask>()
            .HasCheckConstraint("CK_Estimated_Min",
            "estimated_minutes >= 0 AND estimated_minutes <= 10000");

            // --- PROFILE CONFIGURATION ---
            modelBuilder.Entity<Profile>(entity =>
            {
                // Grouping table name and all Check Constraints together
                entity.ToTable("Profile", t =>
                {
                    t.HasCheckConstraint("CK_School_Year", "academic_year IN ('Freshman', 'Sophomore', 'Junior', 'Senior', 'Graduate')");
                    t.HasCheckConstraint("CK_User_Notification", "notification_preferences IN ('instant', 'daily_digest', 'urgent-only', 'none')");
                });

                // Primary Key
                entity.HasKey(e => e.ProfileId).HasName("PK_Profile");

                // Columns
                entity.Property(e => e.ProfileId).HasMaxLength(450).HasColumnName("profile_id");
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100).HasColumnName("display_name");
                entity.HasIndex(e => e.DisplayName).IsUnique().HasDatabaseName("UQ_Profile_DisplayName");

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnName("name");

                entity.Property(e => e.UserCreationDate)
                      .HasColumnType("datetime2(3)")
                      .HasColumnName("user_creation_date")
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.LastLoginTime)
                      .HasColumnType("datetime2(3)")
                      .HasColumnName("last_login_time");

                entity.Property(e => e.AcademicYear).HasMaxLength(50).HasColumnName("academic_year");

                entity.Property(e => e.Timezone).HasMaxLength(50).HasColumnName("timezone").HasDefaultValue("UTC");

                entity.Property(e => e.NotificationPreferences)
                      .HasColumnType("varchar(20)")
                      .HasMaxLength(20)
                      .HasColumnName("notification_preferences")
                      .HasDefaultValue("instant");

                entity.Property(e => e.QuietHoursStart).HasColumnType("time").HasColumnName("quiet_hours_start");
                entity.Property(e => e.QuietHoursEnd).HasColumnType("time").HasColumnName("quiet_hours_end");

                // 1:1 relationship with AspNetUsers (IdentityUser)
                entity.HasOne<IdentityUser>()
                      .WithOne()
                      .HasForeignKey<Profile>(p => p.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Profile_Users");
            });

            // --- COURSE CONFIGURATION ---
            modelBuilder.Entity<Course>(entity =>
            {
                // Unique Constraint (UQ_Course_Study_Session)
                entity.HasIndex(e => new { e.CourseId, e.ProfileId }).IsUnique();

                // Term Check Constraint
                entity.ToTable(t => t.HasCheckConstraint("CK_Course_Term", "term IN ('Spring', 'Summer', 'Fall', 'Winter')"));

                // Academic Year Check Constraint
                entity.ToTable(t => t.HasCheckConstraint("CK_Course_Academic_Year", "academic_year BETWEEN 1900 AND 3000"));

                // Fixed length for Color Label (Hex code like #FFFFFF)
                entity.Property(e => e.ColorLabel).IsFixedLength().HasMaxLength(7);
            });

            // --- ASSIGNMENT CONFIGURATION ---
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
