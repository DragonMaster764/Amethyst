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

        // Student dashboard data
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

        // Admin dashboard data
        public int TotalUsersCount { get; set; }
        public int TotalStudentsCount { get; set; }
        public int TotalGeneralUsersCount { get; set; }
        public int TotalAdminsCount { get; set; }
        public int TotalStudySessionsCount { get; set; }

        // General user dashboard data
        public List<UserTask> GeneralUserTasks { get; set; } = new();
        public List<Reminder> GeneralUserReminders { get; set; } = new();

        public int GeneralUserTaskCount { get; set; }
        public int GeneralUserReminderCount { get; set; }
        public int GeneralUserCompletedTaskCount { get; set; }
        public int GeneralUserOpenTaskCount { get; set; }

        public int GeneralUserTaskCompletionPercentage =>
            GeneralUserTaskCount == 0 ? 0 : (int)Math.Round((double)GeneralUserCompletedTaskCount / GeneralUserTaskCount * 100);

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

            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                return;
            }

            if (IsStudent)
            {
                await LoadStudentDashboardAsync(loggedInUserId);
                return;
            }

            if (IsAdmin)
            {
                await LoadAdminDashboardAsync();
                return;
            }

            if (IsGeneralUser)
            {
                await LoadGeneralUserDashboardAsync(loggedInUserId);
            }
        }

        private async Task LoadStudentDashboardAsync(string loggedInUserId)
        {
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

        private async Task LoadAdminDashboardAsync()
        {
            TotalUsersCount = await _context.Users.CountAsync();

            var userRoles = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                select role.Name
            ).ToListAsync();

            TotalStudentsCount = userRoles.Count(r => r == "Student");
            TotalAdminsCount = userRoles.Count(r => r == "Admin");
            TotalGeneralUsersCount = userRoles.Count(r => r == "User");

            TotalAssignmentsCount = await _context.Assignments.CountAsync();
            TotalTasksDashboardCount = await _context.TaskItems.CountAsync();
            ActiveCoursesCount = await _context.Courses.CountAsync();
            ReminderCount = await _context.Reminders.CountAsync();
            TotalStudySessionsCount = await _context.StudySessions.CountAsync();
        }

        private async Task LoadGeneralUserDashboardAsync(string loggedInUserId)
        {
            GeneralUserTasks = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .Where(t => t.Status != "Completed")
                .OrderBy(t => t.DueAt ?? DateTime.MaxValue)
                .Take(6)
                .ToListAsync();

            GeneralUserReminders = await _context.Reminders
                .Include(r => r.TaskItem)
                .Where(r => r.ProfileId == loggedInUserId)
                .OrderBy(r => r.RemindAt)
                .Take(4)
                .ToListAsync();

            GeneralUserTaskCount = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .CountAsync();

            GeneralUserReminderCount = await _context.Reminders
                .Where(r => r.ProfileId == loggedInUserId)
                .CountAsync();

            GeneralUserCompletedTaskCount = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .Where(t => t.Status == "Completed")
                .CountAsync();

            GeneralUserOpenTaskCount = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .Where(t => t.Status != "Completed")
                .CountAsync();
        }
    }
}