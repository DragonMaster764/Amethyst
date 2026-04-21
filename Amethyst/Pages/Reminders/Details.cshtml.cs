using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Reminders
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Reminder Reminder { get; set; } = default!;

        public string ReminderStatus
        {
            get
            {
                var now = DateTime.Now;

                if (Reminder.RemindAt.Date == now.Date)
                {
                    return "Today";
                }

                if (Reminder.RemindAt < now)
                {
                    return "Past";
                }

                return "Upcoming";
            }
        }

        public string ReminderTitle
        {
            get
            {
                if (Reminder.TargetType == "Assignment" && Reminder.Assignment != null)
                {
                    return Reminder.Assignment.Title;
                }

                if (Reminder.TargetType == "Task" && Reminder.TaskItem != null)
                {
                    return Reminder.TaskItem.Title;
                }

                return "Reminder";
            }
        }

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
    }
}
