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
    public class IndexModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public IndexModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Reminder> Reminder { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Reminder = await _context.Reminders
                .Include(r => r.Assignment)
                .Include(r => r.Profile)
                .Include(r => r.Task).ToListAsync();
        }
    }
}
