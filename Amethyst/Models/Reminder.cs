using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amethyst.Models
{
    public class Reminder
    {
        [Key]
        [Column("reminder_id")]
        public long ReminderId { get; set; }

        [Required]
        [Column("profile_id")]
        public string ProfileId { get; set; }

        [Required]
        [Column("target_type")]
        public string TargetType { get; set; }

        [Column("assignment_id")]
        public int? AssignmentId { get; set; }

        [Column("task_id")]
        public int? TaskId { get; set; }

        [Required]
        [Column("remind_at")]
        public DateTime RemindAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ProfileId))]
        public Profile? Profile { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public Assignment? Assignment { get; set; }

        [ForeignKey(nameof(TaskId))]
        public UserTask? TaskItem { get; set; }
    }
}