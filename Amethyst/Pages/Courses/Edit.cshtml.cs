using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Amethyst.Pages.Courses
{
    [Authorize(Roles = "Student")]
    public class EditModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public EditModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        // Bind a string for the time input to avoid FormatException when binding directly to TimeSpan?
        [BindProperty]
        public string? MeetingTimeString { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            Course = course;

            // Initialize the string used by the <input type="time"> control.
            MeetingTimeString = Course.MeetingTime?.ToString(@"hh\:mm");

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            // Fill out profile id from the logged in user
            Course.ProfileId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Parse the posted time string into the Course.MeetingTime (nullable TimeSpan).
            if (string.IsNullOrWhiteSpace(MeetingTimeString))
            {
                Course.MeetingTime = null;
            }
            else
            {
                // Accept "hh:mm" and "hh:mm:ss" variants coming from the time input.
                var formats = new[] { @"hh\:mm", @"h\:mm", @"hh\:mm\:ss", @"h\:mm\:ss" };
                if (TimeSpan.TryParseExact(MeetingTimeString.Trim(), formats, CultureInfo.InvariantCulture, out var ts))
                {
                    Course.MeetingTime = ts;
                }
                else
                {
                    ModelState.AddModelError(nameof(MeetingTimeString), "Invalid time format. Use HH:MM or HH:MM:SS.");
                }
            }

            // The binder ran before we set ProfileId and may have recorded a validation error.
            // Clear the Course validation state and revalidate so the server-set ProfileId is considered.
            ModelState.ClearValidationState("Course");
            TryValidateModel(Course, "Course");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(Course.CourseId))
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

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
