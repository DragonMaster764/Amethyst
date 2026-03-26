using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Amethyst.Pages.CreateProfile
{
    public class StudentFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentFormModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Bind the Profile fields coming from the form
        [BindProperty]
        public Profile InputProfile { get; set; } = new Profile();

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // Get logged-in user's Identity ID
            string? userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge(); // Not logged in → redirect to login

            // Prevent duplicate profiles
            var existing = _context.Profiles.FirstOrDefault(p => p.ProfileId == userId);
            if (existing != null)
                return RedirectToPage("/EditProfile", new { id = userId });

            // Fill system fields
            InputProfile.ProfileId = userId;
            InputProfile.UserCreationDate = DateTime.UtcNow;

            // Insert into DB
            _context.Profiles.Add(InputProfile);
            _context.SaveChanges();

            // Redirect to a success page (or wherever you want)
            return RedirectToPage("/Success");
        }
    }
}

