using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Amethyst.Models
{
    public class Reminder
    {
        [Key]
        public long ReminderId { get; set; }

        public string ProfileId { get; set; }

        public string TargetType { get; set; }

        public int? AssignmentId { get; set; }

        public int? TaskId { get; set; }

        public DateTime RemindAt { get; set; }

        // Navigation properties
        public Profile Profile { get; set; }

        public Assignment Assignment { get; set; }

        public UserTask TaskItem { get; set; }
    }
}