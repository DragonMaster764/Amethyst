using Amethyst.Data;
using Amethyst.Models; // your Profile model namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace Amethyst.Pages.CreateProfile
{
    public class UserFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UserFormModel(ApplicationDbContext context)
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

            // Generate a GUID for profile_id
            NewProfile.ProfileId = Guid.NewGuid().ToString();

            // Set creation date
            NewProfile.UserCreationDate = DateTime.UtcNow;

            _context.Profiles.Add(NewProfile);
            _context.SaveChanges();

            return RedirectToPage("/Success");
        }
    }
}

