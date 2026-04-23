using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Reminders
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Reminder Reminder { get; set; } = new Reminder();

        public SelectList AssignmentOptions { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList TaskOptions { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList TargetTypeOptions { get; set; } = new SelectList(new[]
        {
            new SelectListItem { Value = "Assignment", Text = "Assignment" },
            new SelectListItem { Value = "Task", Text = "Task" }
        }, "Value", "Text");

        public async Task<IActionResult> OnGetAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                return Challenge();
            }

            var now = DateTime.Now;

            Reminder = new Reminder
            {
                RemindAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0)
            };

            await LoadDropdownsAsync(loggedInUserId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                return Challenge();
            }

            Reminder.ProfileId = loggedInUserId;
            ModelState.Remove("Reminder.ProfileId");

            if (Reminder.RemindAt == default)
            {
                Reminder.RemindAt = DateTime.Now;
            }

            if (string.IsNullOrWhiteSpace(Reminder.TargetType))
            {
                ModelState.AddModelError("Reminder.TargetType", "Please select a target type.");
            }
            else if (Reminder.TargetType == "Assignment")
            {
                if (!Reminder.AssignmentId.HasValue)
                {
                    ModelState.AddModelError("Reminder.AssignmentId", "Please select an assignment.");
                }

                Reminder.TaskId = null;
            }
            else if (Reminder.TargetType == "Task")
            {
                if (!Reminder.TaskId.HasValue)
                {
                    ModelState.AddModelError("Reminder.TaskId", "Please select a task.");
                }

                Reminder.AssignmentId = null;
            }
            else
            {
                ModelState.AddModelError("Reminder.TargetType", "Please select a valid target type.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(loggedInUserId);
                return Page();
            }

            Reminder.RemindAt = new DateTime(
                Reminder.RemindAt.Year,
                Reminder.RemindAt.Month,
                Reminder.RemindAt.Day,
                Reminder.RemindAt.Hour,
                Reminder.RemindAt.Minute,
                0
            );

            _context.Reminders.Add(Reminder);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private async Task LoadDropdownsAsync(string loggedInUserId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course != null && a.Course.ProfileId == loggedInUserId)
                .OrderBy(a => a.Title)
                .ToListAsync();

            var tasks = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .OrderBy(t => t.Title)
                .ToListAsync();

            AssignmentOptions = new SelectList(assignments, "AssignmentId", "Title");
            TaskOptions = new SelectList(tasks, "TaskId", "Title");
        }
    }
}