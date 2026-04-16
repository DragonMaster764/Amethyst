using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Study_Session
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public StudySession StudySession { get; set; } = default!;

        public SelectList CourseOptions { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public async Task<IActionResult> OnGetAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                return Challenge();
            }

            await LoadCourseOptionsAsync(loggedInUserId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                return Challenge();
            }

            StudySession.ProfileId = loggedInUserId;

            if (StudySession.EndTime.HasValue && StudySession.EndTime.Value <= StudySession.StartTime)
            {
                ModelState.AddModelError("StudySession.EndTime", "End time must be later than the start time.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCourseOptionsAsync(loggedInUserId);
                return Page();
            }

            _context.StudySessions.Add(StudySession);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadCourseOptionsAsync(string loggedInUserId)
        {
            var courses = await _context.Courses
                .Where(c => c.ProfileId == loggedInUserId)
                .OrderBy(c => c.Title)
                .ToListAsync();

            CourseOptions = new SelectList(courses, "CourseId", "Title");
        }
    }
}