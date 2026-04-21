using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Task_Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UserTask> TaskItems { get; set; } = new List<UserTask>();

        public async Task OnGetAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var allTasks = await _context.TaskItems.ToListAsync();

            Console.WriteLine($"Logged in user id: {loggedInUserId}");
            Console.WriteLine($"All task count: {allTasks.Count}");

            foreach (var task in allTasks)
            {
                Console.WriteLine($"TaskId={task.TaskId}, Title={task.Title}, ProfileId={task.ProfileId}");
            }

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                TaskItems = new List<UserTask>();
                return;
            }

            TaskItems = await _context.TaskItems
                .Where(t => t.ProfileId == loggedInUserId)
                .OrderBy(t => t.DueAt ?? DateTime.MaxValue)
                .ToListAsync();
        }
    }
}