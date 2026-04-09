using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Study_Session
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<StudySession> StudySession { get; set; } = default!;

        [BindProperty]
        public int SessionId { get; set; }

        [BindProperty]
        public short? PlannedMinutesInput { get; set; }

        [BindProperty]
        public short? ActualMinutesInput { get; set; }

        [BindProperty]
        public string? NotesInput { get; set; }

        public async Task OnGetAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            StudySession = await _context.StudySessions
                .Include(s => s.Course)
                .Include(s => s.Profile)
                .Where(s => s.ProfileId == loggedInUserId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdatePlannedMinutesAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var session = await _context.StudySessions
                .FirstOrDefaultAsync(s => s.SessionId == SessionId && s.ProfileId == loggedInUserId);

            if (session == null)
            {
                return NotFound();
            }

            session.PlannedMinutes = PlannedMinutesInput;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateActualMinutesAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var session = await _context.StudySessions
                .FirstOrDefaultAsync(s => s.SessionId == SessionId && s.ProfileId == loggedInUserId);

            if (session == null)
            {
                return NotFound();
            }

            session.ActualMinutes = ActualMinutesInput;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateNotesAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var session = await _context.StudySessions
                .FirstOrDefaultAsync(s => s.SessionId == SessionId && s.ProfileId == loggedInUserId);

            if (session == null)
            {
                return NotFound();
            }

            session.Notes = string.IsNullOrWhiteSpace(NotesInput)
                ? null
                : NotesInput.Trim().Length > 500
                    ? NotesInput.Trim().Substring(0, 500)
                    : NotesInput.Trim();

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}