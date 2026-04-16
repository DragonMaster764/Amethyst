using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amethyst.Models
{
    [Table("Tasks")]
    public class UserTask
    {
        [Key]
        [Column("task_id")]
        public int TaskId { get; set; }

        [Required]
        [Column("profile_id")]
        public string ProfileId { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("due_at")]
        public DateTime? DueAt { get; set; }

        [Column("status")]
        public string Status { get; set; } = string.Empty;

        [Column("priority")]
        public string Priority { get; set; } = string.Empty;

        [Column("estimated_minutes")]
        public short? EstimatedMinutes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(ProfileId))]
        public Profile? Profile { get; set; }
    }
}