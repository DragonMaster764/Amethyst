using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Amethyst.Models
{
    public class StudySession
    {
        public int SessionId { get; set; }

        public string ProfileId { get; set; }

        public int? CourseId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public short? PlannedMinutes { get; set; }

        public short? ActualMinutes { get; set; }

        public string Notes { get; set; }

        // Navigation properties
        public Profile Profile { get; set; }

        public Course Course { get; set; }
    }
}