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

namespace Amethyst.Pages.Study_Session
{
    public class EditModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public EditModel(Amethyst.Data.ApplicationDbContext context)
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

            var studysession =  await _context.StudySessions.FirstOrDefaultAsync(m => m.SessionId == id);
            if (studysession == null)
            {
                return NotFound();
            }
            StudySession = studysession;
           ViewData["CourseId"] = new SelectList(_context.Courses, "CourseId", "ProfileId");
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

            _context.Attach(StudySession).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudySessionExists(StudySession.SessionId))
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

        private bool StudySessionExists(int id)
        {
            return _context.StudySessions.Any(e => e.SessionId == id);
        }
    }
}
