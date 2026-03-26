using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Amethyst.Pages.CreateProfile
{
    public class UserFormModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserFormModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Profile InputProfile { get; set; } = new Profile();

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            string? userId = _userManager.GetUserId(User);

            if (userId == null)
                return Challenge(); // Redirect to login

            // Prevent duplicate profiles
            var existing = _context.Profiles.FirstOrDefault(p => p.ProfileId == userId);
            if (existing != null)
                return RedirectToPage("/EditProfile", new { id = userId });

            // System fields
            InputProfile.ProfileId = userId;
            InputProfile.UserCreationDate = DateTime.UtcNow;

            // ⭐ Regular users do NOT have an academic year
            InputProfile.AcademicYear = null;

            // Insert into DB
            _context.Profiles.Add(InputProfile);
            _context.SaveChanges();

            return RedirectToPage("/Success");
        }
    }
}


