using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Amethyst.Models
{
    public class Tasks
    {
        public int TaskId { get; set; }

        public string ProfileId { get; set; }

        public string Title { get; set; }

        public DateTime DueAt { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public short? EstimatedMinutes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Profile Profile { get; set; }
    }
}

