using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amethyst.Models
{
    [Table("Assignment")]
    public class Assignment
    {
        [Key]
        [Column("assignment_id")]
        public int AssignmentId { get; set; }

        [Column("course_id")]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Not Started";

        [Required]
        [MaxLength(15)]
        [Column("priority")]
        public string Priority { get; set; } = "Low";

        [Range(0, 10000)]
        [Column("estimated_minutes")]
        public short? EstimatedMinutes { get; set; }

        [Column("points", TypeName = "decimal(6, 2)")]
        public decimal? Points { get; set; }

        [Column("total_points", TypeName = "decimal(6, 2)")]
        public decimal? TotalPoints { get; set; }

        [Column("raw_percentage", TypeName = "decimal(5, 2)")]
        public decimal? RawPercentage { get; set; }

        [MaxLength(2)]
        [Column("grade")]
        public string? Grade { get; set; }

        [Column("updated_time")]
        public DateTime? UpdatedTime { get; set; }

        // Navigation Property for the Foreign Key
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
    }
}
