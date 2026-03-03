using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Amethyst.Data;
using Amethyst.Models;

namespace Amethyst.Pages.Task_Pages
{
    public class CreateModel : PageModel
    {
        private readonly Amethyst.Data.ApplicationDbContext _context;

        public CreateModel(Amethyst.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["ProfileId"] = new SelectList(_context.Set<Profile>(), "ProfileId", "ProfileId");
            return Page();
        }

        [BindProperty]
        public Tasks Tasks { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Tasks.Add(Tasks);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
