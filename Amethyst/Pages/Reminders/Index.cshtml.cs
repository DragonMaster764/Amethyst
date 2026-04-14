using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Reminders
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Reminder> Reminder { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Reminder = await _context.Reminders
                .Include(r => r.Assignment)
                .Include(r => r.TaskItem)
                .Where(r => r.ProfileId == loggedInUserId)
                .OrderBy(r => r.RemindAt)
                .ToListAsync();
        }
    }
}