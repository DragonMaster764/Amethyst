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
    public class IndexModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public IndexModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UserTask> TaskItems { get;set; } = default!;

        public async Task OnGetAsync()
        {
            //TaskItems = await _context.TaskItems.Include(t => t.Profile).ToListAsync();
            TaskItems = await _context.TaskItems.ToListAsync();
        }
    }
}
