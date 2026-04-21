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
        public List<Course> ActiveCourses { get; set; } = new();

        public int TotalAssignmentsCount { get; set; }
        public int TotalTasksDashboardCount { get; set; }
        public int ActiveCoursesCount { get; set; }
        public int ReminderCount { get; set; }

        public int TotalTasksCount { get; set; }
        public int CompletedTasksCount { get; set; }
        public int CompletedTasksPercentage =>
            TotalTasksCount == 0 ? 0 : (int)Math.Round((double)CompletedTasksCount / TotalTasksCount * 100);

        public List<Course> VisibleActiveCourses =>
            ActiveCourses.Take(4).ToList();

        public int RemainingActiveCoursesCount =>
            Math.Max(0, ActiveCourses.Count - 4);
        
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

            TodayTasks = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .Where(t => t.Status != "Completed")
                .OrderBy(t => t.DueAt ?? DateTime.MaxValue)
                .Take(6)
                .ToListAsync();

            TotalTasksDashboardCount = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .CountAsync();

            UpcomingAssignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course != null && a.Course.ProfileId == loggedInUserId)
                .Where(a => a.Status != "Completed")
                .Where(a => a.DueDate.HasValue)
                .OrderBy(a => a.DueDate)
                .Take(4)
                .ToListAsync();

            TotalAssignmentsCount = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => a.Course != null && a.Course.ProfileId == loggedInUserId)
                .CountAsync();

            var allCourses = await _context.Courses
                .Where(c => c.ProfileId == loggedInUserId)
                .ToListAsync();

            int GetTermRank(string? term)
            {
                return term?.Trim().ToLower() switch
                {
                    "spring" => 4,
                    "summer" => 3,
                    "fall" => 2,
                    "winter" => 1,
                    _ => 0
                };
            }

            var mostCurrentGroup = allCourses
                .OrderByDescending(c => c.AcademicYear)
                .ThenByDescending(c => GetTermRank(c.Term))
                .Select(c => new { c.AcademicYear, c.Term })
                .FirstOrDefault();

            if (mostCurrentGroup != null)
            {
                ActiveCourses = allCourses
                    .Where(c => c.AcademicYear == mostCurrentGroup.AcademicYear && c.Term == mostCurrentGroup.Term)
                    .OrderBy(c => c.Title)
                    .ToList();
            }
            else
            {
                ActiveCourses = new List<Course>();
            }

            ActiveCoursesCount = ActiveCourses.Count;

            ReminderCount = await _context.Reminders
                .Where(r => r.ProfileId == loggedInUserId)
                .CountAsync();

            UpcomingStudySessions = await _context.StudySessions
                .Where(s => s.ProfileId == loggedInUserId)
                .Where(s => s.StartTime >= DateTime.Now)
                .OrderBy(s => s.StartTime)
                .Take(2)
                .ToListAsync();

            if (!UpcomingStudySessions.Any())
            {
                UpcomingStudySessions = await _context.StudySessions
                    .Where(s => s.ProfileId == loggedInUserId)
                    .OrderBy(s => s.StartTime)
                    .Take(2)
                    .ToListAsync();
            }

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