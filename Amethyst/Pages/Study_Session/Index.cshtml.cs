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
    public class IndexModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public IndexModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<StudySession> StudySession { get;set; } = default!;

        public async Task OnGetAsync()
        {
            StudySession = await _context.StudySessions
                .Include(s => s.Course)
                .Include(s => s.Profile).ToListAsync();
        }
    }
}
