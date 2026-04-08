using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Task_Pages
{
    public class DeleteModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public DeleteModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public UserTask TaskItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasks = await _context.TaskItems.FirstOrDefaultAsync(m => m.TaskId == id);

            if (tasks == null)
            {
                return NotFound();
            }
            else
            {
                TaskItem = tasks;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasks = await _context.TaskItems.FindAsync(id);
            if (tasks != null)
            {
                TaskItem = tasks;
                _context.TaskItems.Remove(TaskItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
