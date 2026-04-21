using System.Security.Claims;
using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Amethyst.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public bool IsStudent { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsGeneralUser { get; set; }

        public List<UserTask> TodayTasks { get; set; } = new();
        public List<Assignment> UpcomingAssignments { get; set; } = new();
        public List<StudySession> UpcomingStudySessions { get; set; } = new();

        public int AssignmentsDueThisWeekCount { get; set; }
        public int TasksDueTodayCount { get; set; }
        public int ActiveCoursesCount { get; set; }
        public int ReminderCount { get; set; }

        public int TotalTasksCount { get; set; }
        public int CompletedTasksCount { get; set; }
        public int CompletedTasksPercentage =>
            TotalTasksCount == 0 ? 0 : (int)Math.Round((double)CompletedTasksCount / TotalTasksCount * 100);

       public async Task OnGetAsync()
        {
            await LoadDashboardDataAsync();
        }

        public async Task<IActionResult> OnPostCompleteTaskAsync(int taskId)
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                return Challenge();
            }

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.ProfileId == loggedInUserId);

            if (task == null)
            {
                return NotFound();
            }

            task.Status = "Completed";
            task.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        private async Task LoadDashboardDataAsync()
        {
            IsAdmin = User.IsInRole("Admin");
            IsStudent = User.IsInRole("Student");
            IsGeneralUser = !IsAdmin && !IsStudent;

            if (!IsStudent)
            {
                return;
            }

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                return;
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var endOfWeek = today.AddDays(7);

            TodayTasks = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .Where(t => t.Status != "Completed")
                .Where(t => t.DueAt.HasValue && t.DueAt.Value >= today && t.DueAt.Value < tomorrow)
                .OrderBy(t => t.DueAt)
                .Take(6)
                .ToListAsync();

            TasksDueTodayCount = TodayTasks.Count;

            if (!TodayTasks.Any())
            {
                TodayTasks = await _context.TaskItems
                    .Where(t => t.ProfileId == loggedInUserId)
                    .Where(t => t.Status != "Completed")
                    .OrderBy(t => t.DueAt ?? DateTime.MaxValue)
                    .Take(6)
                    .ToListAsync();
            }

            UpcomingAssignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course != null && a.Course.ProfileId == loggedInUserId)
                .Where(a => a.Status != "Completed")
                .Where(a => a.DueDate.HasValue)
                .OrderBy(a => a.DueDate)
                .Take(4)
                .ToListAsync();

            AssignmentsDueThisWeekCount = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course != null && a.Course.ProfileId == loggedInUserId)
                .Where(a => a.Status != "Completed")
                .Where(a => a.DueDate.HasValue && a.DueDate.Value >= today && a.DueDate.Value <= endOfWeek)
                .CountAsync();

            ActiveCoursesCount = await _context.Courses
                .Where(c => c.ProfileId == loggedInUserId)
                .CountAsync();

            ReminderCount = await _context.Reminders
                .Where(r => r.ProfileId == loggedInUserId)
                .CountAsync();

            UpcomingStudySessions = await _context.StudySessions
                .Where(s => s.ProfileId == loggedInUserId)
                .Where(s => s.StartTime >= DateTime.Now)
                .OrderBy(s => s.StartTime)
                .Take(2)
                .ToListAsync();

            TotalTasksCount = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .CountAsync();

            CompletedTasksCount = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .Where(t => t.Status == "Completed")
                .CountAsync();
        }
    }
}