using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amethyst.Models
{
    [Table("Course")]
    public class Course
    {
        [Key]
        [Column("course_id")]
        public int CourseId { get; set; }

        [Required]
        [Column("profile_id")]
        public string ProfileId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Column("term")]
        public string Term { get; set; } = string.Empty;

        [Column("academic_year")]
        public short AcademicYear { get; set; }

        [Column("meeting_time")]
        public TimeSpan? MeetingTime { get; set; }

        [MaxLength(100)]
        [Column("instructor_name")]
        public string? InstructorName { get; set; }

        [MaxLength(7)]
        [Column("color_label")]
        public string? ColorLabel { get; set; }

        // Navigation Properties
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
