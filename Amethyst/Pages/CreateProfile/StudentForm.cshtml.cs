using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Amethyst.Pages.CreateProfile
{
    public class StudentFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public StudentFormModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Profile NewProfile { get; set; } = new Profile();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            NewProfile.ProfileId = Guid.NewGuid().ToString();
            NewProfile.UserCreationDate = DateTime.UtcNow;

            _context.Profiles.Add(NewProfile);
            _context.SaveChanges();

            return RedirectToPage("/Success");
        }
    }
}

