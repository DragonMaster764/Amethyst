using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Study_Session
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public StudySession StudySession { get; set; } = default!;

        public bool IsPastSession =>
            StudySession.EndTime.HasValue && StudySession.EndTime.Value < DateTime.Now;

        public int? SessionLengthMinutes
        {
            get
            {
                if (!StudySession.EndTime.HasValue)
                {
                    return null;
                }

                var totalMinutes = (int)(StudySession.EndTime.Value - StudySession.StartTime).TotalMinutes;
                return totalMinutes > 0 ? totalMinutes : null;
            }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var studysession = await _context.StudySessions
                .Include(s => s.Course)
                .Include(s => s.Profile)
                .FirstOrDefaultAsync(s => s.SessionId == id && s.ProfileId == loggedInUserId);

            if (studysession == null)
            {
                return NotFound();
            }

            StudySession = studysession;
            return Page();
        }
    }
}