using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Reminders
{
    public class DetailsModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public DetailsModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Reminder Reminder { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reminder = await _context.Reminders.FirstOrDefaultAsync(m => m.ReminderId == id);
            if (reminder == null)
            {
                return NotFound();
            }
            else
            {
                Reminder = reminder;
            }
            return Page();
        }
    }
}
