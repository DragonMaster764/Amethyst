using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Amethyst.Models
{
    [Table("Reminder")]
    public class Reminder
    {
        [Key]
        [Column("reminder_id")]
        public long ReminderId { get; set; }

        [Required]
        [Column("profile_id")]
        public string ProfileId { get; set; } = string.Empty;

        
        [Required]
        [Column("target_type")]
        public string TargetType { get; set; } = string.Empty;

        [Column("assignment_id")]
        public int? AssignmentId { get; set; }

        [Column("task_id")]
        public int? TaskId { get; set; }

        
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