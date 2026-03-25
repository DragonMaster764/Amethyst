using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Reminders
{
    public class CreateModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public CreateModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["AssignmentId"] = new SelectList(_context.Assignments, "AssignmentId", "Priority");
        ViewData["ProfileId"] = new SelectList(_context.Set<Profile>(), "ProfileId", "ProfileId");
        ViewData["TaskId"] = new SelectList(_context.TaskItems, "TaskId", "TaskId");
            return Page();
        }

        [BindProperty]
        public Reminder Reminder { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Reminders.Add(Reminder);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
