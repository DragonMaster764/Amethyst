using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Task_Pages
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public UserTask TaskItem { get; set; } = new();

        public IActionResult OnGet()
        {
            TaskItem = new UserTask
            {
                DueAt = DateTime.Now,
                Status = "Not Started",
                Priority = "Medium"
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(loggedInUserId))
            {
                return Challenge();
            }

            TaskItem.ProfileId = loggedInUserId;
            TaskItem.CreatedAt = DateTime.Now;
            TaskItem.UpdatedAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(TaskItem.Status))
            {
                TaskItem.Status = "Not Started";
            }

            if (string.IsNullOrWhiteSpace(TaskItem.Priority))
            {
                TaskItem.Priority = "Medium";
            }

            ModelState.ClearValidationState(nameof(TaskItem));
            TryValidateModel(TaskItem, nameof(TaskItem));

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.TaskItems.Add(TaskItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}