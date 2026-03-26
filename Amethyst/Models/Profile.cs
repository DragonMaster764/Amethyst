using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Amethyst.Models
{
    [Table("Profile")]
    public class Profile
    {
        [Key]
        [Column("profile_id")]
        [MaxLength(450)]
        public string ProfileId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("user_creation_date", TypeName = "datetime2(3)")]
        public DateTime UserCreationDate { get; set; }

        [Column("last_login_time", TypeName = "datetime2(3)")]
        public DateTime? LastLoginTime { get; set; }

        [MaxLength(50)]
        [Column("academic_year")]
        public string? AcademicYear { get; set; }

        [MaxLength(50)]
        [Column("timezone")]
        public string Timezone { get; set; } = "UTC";

        [Column("notification_preferences", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string NotificationPreferences { get; set; } = "instant";

        [Column("quiet_hours_start", TypeName = "time")]
        public TimeSpan? QuietHoursStart { get; set; }

        [Column("quiet_hours_end", TypeName = "time")]
        public TimeSpan? QuietHoursEnd { get; set; }

        [NotMapped]
        public new IdentityUser? User { get; set; }

        // Navigation to courses owned by this profile
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}