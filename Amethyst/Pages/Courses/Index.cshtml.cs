using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Amethyst.Pages.Courses
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public IndexModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CourseGroup> CourseGroups { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courses = await _context.Courses
                .Where(c => c.ProfileId == userId)
                .OrderBy(c => c.AcademicYear)
                .ThenBy(c => c.Term)
                .ToListAsync();

            CourseGroups = courses
                .GroupBy(c => new { c.Term, c.AcademicYear })
                .OrderByDescending(g => g.Key.AcademicYear)
                .ThenByDescending(g => g.Key.Term)
                .Select(g => new CourseGroup
                {
                    Term = g.Key.Term,
                    AcademicYear = g.Key.AcademicYear,
                    Courses = g.ToList()
                })
                .ToList();
        }


        //Holds each course group (term + academic year) and the courses that belong to that group
        public class CourseGroup
        {
            public string Term { get; set; } = string.Empty;
            public short AcademicYear { get; set; }
            public List<Course> Courses { get; set; } = new();
            public string GroupTitle => $"{Term} {AcademicYear}";
        }
    }
}
