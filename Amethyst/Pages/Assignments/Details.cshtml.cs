using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Assignments
{
    public class DetailsModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public DetailsModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Assignment Assignment { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments.FirstOrDefaultAsync(m => m.AssignmentId == id);
            if (assignment == null)
            {
                return NotFound();
            }
            else
            {
                Assignment = assignment;
            }
            return Page();
        }
    }
}
