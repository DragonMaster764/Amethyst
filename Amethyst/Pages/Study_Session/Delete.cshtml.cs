using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Study_Session
{
    public class DeleteModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public DeleteModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public StudySession StudySession { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studysession = await _context.StudySessions.FirstOrDefaultAsync(m => m.SessionId == id);

            if (studysession == null)
            {
                return NotFound();
            }
            else
            {
                StudySession = studysession;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studysession = await _context.StudySessions.FindAsync(id);
            if (studysession != null)
            {
                StudySession = studysession;
                _context.StudySessions.Remove(StudySession);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
