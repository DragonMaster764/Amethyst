using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Amethyst.Pages.Courses
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public IndexModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Course> Course { get;set; } = default!;

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Course = await _context.Courses
                .Where(c => c.ProfileId == userId)
                .ToListAsync();
        }
    }
}
