using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Reminders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reminder Reminder { get; set; } = default!;

        public SelectList AssignmentOptions { get; set; } = default!;
        public SelectList TaskOptions { get; set; } = default!;
        public SelectList TargetTypeOptions { get; set; } = default!;

       public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.ReminderId == id && r.ProfileId == loggedInUserId);

            if (reminder == null)
            {
                return NotFound();
            }

            Reminder = reminder;
            await LoadDropdownsAsync(loggedInUserId);

            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingReminder = await _context.Reminders
                .FirstOrDefaultAsync(r => r.ReminderId == Reminder.ReminderId && r.ProfileId == loggedInUserId);

            if (existingReminder == null)
            {
                return NotFound();
            }

            if (Reminder.TargetType == "Assignment")
            {
                if (!Reminder.AssignmentId.HasValue)
                {
                    ModelState.AddModelError("Reminder.AssignmentId", "Please select an assignment.");
                }

                existingReminder.TargetType = "Assignment";
                existingReminder.AssignmentId = Reminder.AssignmentId;
                existingReminder.TaskId = null;
            }
            else if (Reminder.TargetType == "Task")
            {
                if (!Reminder.TaskId.HasValue)
                {
                    ModelState.AddModelError("Reminder.TaskId", "Please select a task.");
                }

                existingReminder.TargetType = "Task";
                existingReminder.TaskId = Reminder.TaskId;
                existingReminder.AssignmentId = null;
            }
            else
            {
                ModelState.AddModelError("Reminder.TargetType", "Please select a valid target type.");
            }

            existingReminder.RemindAt = Reminder.RemindAt;

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(loggedInUserId);
                return Page();
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
        private async Task LoadDropdownsAsync(string? profileId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course != null && a.Course.ProfileId == profileId)
                .OrderBy(a => a.Title)
                .ToListAsync();

            var tasks = await _context.TaskItems
                .Where(t => t.ProfileId == profileId)
                .OrderBy(t => t.Title)
                .ToListAsync();

            AssignmentOptions = new SelectList(assignments, "AssignmentId", "Title");
            TaskOptions = new SelectList(tasks, "TaskId", "Title");
            TargetTypeOptions = new SelectList(new[]
            {
                new { Value = "Assignment", Text = "Assignment" },
                new { Value = "Task", Text = "Task" }
            }, "Value", "Text");
        }
    }
}