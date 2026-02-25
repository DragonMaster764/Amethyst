using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Amethyst.Models
{
    public class Student
    {
        public long StudentID { get; set; }

        public string ProfileId { get; set; }

        public string SchoolName { get; set; }

        public string StudentType { get; set; }

        public string Major { get; set; }

        public string Minor { get; set; }

        public string SchoolDistrict { get; set; }

        public short? GraduationYear { get; set; }

        public string UsState { get; set; }

        // Navigation property
        public Profile Profile { get; set; }
    }
}