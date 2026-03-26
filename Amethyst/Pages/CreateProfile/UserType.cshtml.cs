using Amethyst.Data;
using Amethyst.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Amethyst.Pages.CreateProfile
{
    public class UserTypeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserTypeModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string? Message { get; set; }

        public IActionResult OnGet()
        {
            string? userId = _userManager.GetUserId(User);

            // ⭐ NOT LOGGED IN → send to login page automatically
            if (userId == null)
            {
                Message = "You must be logged in to create a profile.";
                return Challenge(); // Identity handles login routing
            }

            // ⭐ LOGGED IN → check if profile exists
            var existingProfile = _context.Profiles.FirstOrDefault(p => p.ProfileId == userId);

            if (existingProfile != null)
            {
                Message = "Profile already exists — redirecting you to your profile.";
                return RedirectToPage("/EditProfile", new { id = userId });
            }

            // ⭐ LOGGED IN + NO PROFILE → show UserType page
            return Page();
        }
    }
}

