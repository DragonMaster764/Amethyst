using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Amethyst.Pages.Assignments
{
    public class CreateModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public CreateModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courses = _context.Courses.Where(c => c.ProfileId == userID).ToList();

            ViewData["CourseId"] = new SelectList(courses, "CourseId", "Title");
            return Page();
        }

        [BindProperty]
        public Assignment Assignment { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            Assignment.UpdatedTime = DateTime.Now;

            // Load and attach the Course navigation property so EF has the relationship object
            var course = await _context.Courses.FindAsync(Assignment.CourseId);
            if (course == null)
            {
                // Course not found — add a model error and redisplay
                ModelState.AddModelError("Assignment.CourseId", "Selected course not found.");
                var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var courses = _context.Courses.Where(c => c.ProfileId == userID).ToList();
                ViewData["CourseId"] = new SelectList(courses, "CourseId", "Title", Assignment?.CourseId);
                return Page();
            }

            Assignment.Course = course;

            // Clear the  validation state and revalidate.
            ModelState.ClearValidationState("Assignment");
            TryValidateModel(Assignment, "Assignment");

            if (!ModelState.IsValid)
            {
                // Repopulate courses for the select when returning the page
                var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var courses = _context.Courses.Where(c => c.ProfileId == userID).ToList();
                ViewData["CourseId"] = new SelectList(courses, "CourseId", "Title", Assignment?.CourseId);

                return Page();
            }

            _context.Assignments.Add(Assignment);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
