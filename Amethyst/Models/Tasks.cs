using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Amethyst.Models
{
    [Table("Tasks")]
    public class UserTask
    {
        [Key]
        [Column("task_id")]
        public int TaskId { get; set; }

        [Column("profile_id")]
        public string ProfileId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("due_at")]
        public DateTime? DueAt { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("priority")]
        public string Priority { get; set; }

        [Column("estimated_minutes")]
        public short? EstimatedMinutes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        //[ForeignKey("ProfileId")]
        public Profile? Profile { get; set; }
    }
}