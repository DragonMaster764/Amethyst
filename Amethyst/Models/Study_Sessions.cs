using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amethyst.Models
{
    [Table("Study_Session")]
    public class StudySession
    {
        [Key]
        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("profile_id")]
        public string? ProfileId { get; set; }

        [Column("course_id")]
        public int? CourseId { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("planned_minutes")]
        public short? PlannedMinutes { get; set; }

        [Column("actual_minutes")]
        public short? ActualMinutes { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [ForeignKey("ProfileId")]
        public Profile? Profile { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
    }
}