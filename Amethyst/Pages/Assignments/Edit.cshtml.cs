using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Amethyst.Pages.Assignments
{
    public class EditModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public EditModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Assignment Assignment { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment =  await _context.Assignments.FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignment == null)
            {
                return NotFound();
            }
            Assignment = assignment;

            var userID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courses = _context.Courses.Where(c => c.ProfileId == userID).ToList();

            ViewData["CourseId"] = new SelectList(courses, "CourseId", "Title");

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
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
                return Page();
            }

            _context.Attach(Assignment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssignmentExists(Assignment.AssignmentId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.AssignmentId == id);
        }
    }
}
