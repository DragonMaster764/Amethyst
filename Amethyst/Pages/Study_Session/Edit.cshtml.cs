using System.Security.Claims;
using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Amethyst.Pages.Study_Session
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public StudySession StudySession { get; set; } = default!;

        public SelectList CourseOptions { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var session = await _context.StudySessions
                .FirstOrDefaultAsync(s => s.SessionId == id && s.ProfileId == loggedInUserId);

            if (session == null)
            {
                return NotFound();
            }

            StudySession = session;
            await LoadCourseOptionsAsync(loggedInUserId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingSession = await _context.StudySessions
                .FirstOrDefaultAsync(s => s.SessionId == StudySession.SessionId && s.ProfileId == loggedInUserId);

            if (existingSession == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadCourseOptionsAsync(loggedInUserId);
                return Page();
            }

            existingSession.CourseId = StudySession.CourseId;
            existingSession.StartTime = StudySession.StartTime;
            existingSession.EndTime = StudySession.EndTime;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadCourseOptionsAsync(string? loggedInUserId)
        {
            var courses = await _context.Courses
                .Where(c => c.ProfileId == loggedInUserId)
                .OrderBy(c => c.Title)
                .ToListAsync();

            CourseOptions = new SelectList(courses, "CourseId", "Title");
        }
    }
}