using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Reminders
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reminder Reminder { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reminder = await _context.Reminders
                .Include(r => r.Assignment)
                .Include(r => r.TaskItem)
                .FirstOrDefaultAsync(r => r.ReminderId == id && r.ProfileId == loggedInUserId);

            if (reminder == null)
            {
                return NotFound();
            }

            Reminder = reminder;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.ReminderId == id && r.ProfileId == loggedInUserId);

            if (reminder != null)
            {
                _context.Reminders.Remove(reminder);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}