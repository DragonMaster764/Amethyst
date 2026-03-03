using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Task_Pages
{
    public class EditModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public EditModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Tasks Tasks { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tasks =  await _context.Tasks.FirstOrDefaultAsync(m => m.TaskId == id);
            if (tasks == null)
            {
                return NotFound();
            }
            Tasks = tasks;
           ViewData["ProfileId"] = new SelectList(_context.Set<Profile>(), "ProfileId", "ProfileId");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Tasks).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TasksExists(Tasks.TaskId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool TasksExists(int id)
        {
            return _context.Tasks.Any(e => e.TaskId == id);
        }
    }
}
